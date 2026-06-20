import { Component, ElementRef, AfterViewInit, OnDestroy, input, output, effect } from '@angular/core';
import * as L from 'leaflet';

// Fix Leaflet default icon paths (webpack/vite don't resolve them automatically)
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

@Component({
  selector: 'app-map-picker',
  imports: [],
  template: `
    <div class="map-container"></div>
    @if (mode() === 'polygon') {
      <div class="polygon-toolbar">
        <span class="hint">Кликайте по карте, чтобы добавить вершины (мин. 3)</span>
        <button class="btn-poly" (click)="clearPolygon()">✕ Очистить</button>
        <span class="count">{{ vertices().length }} точек</span>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }
    .map-container { width: 100%; height: 300px; border-radius: 8px; border: 1px solid #ddd; }
    .polygon-toolbar {
      display: flex; align-items: center; gap: 0.75rem; margin-top: 0.5rem;
      font-size: 0.8rem; color: #555;
    }
    .btn-poly {
      background: #e53935; color: white; border: none; padding: 0.25rem 0.75rem;
      border-radius: 4px; cursor: pointer; font-size: 0.8rem;
    }
    .hint { flex: 1; }
    .count { font-weight: 600; }
  `]
})
export class MapPicker implements AfterViewInit, OnDestroy {
  private map!: L.Map;
  private tileLayer!: L.TileLayer;

  // For point mode
  private pointMarker!: L.Marker;

  // For polygon mode
  private vertexMarkers: L.Marker[] = [];
  private polygonLine!: L.Polyline;
  private polygonFill!: L.Polygon;

  // Geofence overlay layers
  private geofenceLayers: L.Polygon[] = [];
  // Route stop markers
  private stopMarkers: L.Marker[] = [];

  readonly centerLat = input<number>(42.8746);
  readonly centerLon = input<number>(74.5698);
  readonly editable = input<boolean>(true);
  readonly mode = input<'point' | 'polygon'>('point');
  readonly height = input<number>(300);

  // Geofence overlays to show on the map
  readonly geofenceOverlays = input<{ name: string; type: string; vertices: { lat: number; lon: number }[] }[]>([]);

  // Additional markers to display (e.g., route stops)
  readonly markers = input<{ lat: number; lon: number; label: string }[]>([]);

  readonly centerChanged = output<{ lat: number; lon: number }>();
  readonly vertices = input<{ lat: number; lon: number }[]>([]);
  readonly verticesChanged = output<{ lat: number; lon: number }[]>();

  private loadingFromParent = false;

  constructor(private el: ElementRef<HTMLElement>) {
    effect(() => {
      const verts = this.vertices();
      if (this.map && this.mode() === 'polygon' && verts.length > 0 && !this.loadingFromParent) {
        this.loadingFromParent = true;
        this.loadVerticesIntoMap(verts);
        this.loadingFromParent = false;
      }
    });
    // Watch for geofence overlay changes
    effect(() => {
      const overlays = this.geofenceOverlays();
      if (this.map) {
        this.drawGeofenceOverlays(overlays);
      }
    });
    // Watch for marker changes
    effect(() => {
      const m = this.markers();
      if (this.map) {
        this.drawMarkers(m);
      }
    });
  }

  ngAfterViewInit() {
    setTimeout(() => this.initMap(), 100);
  }

  private initMap() {
    const container = this.el.nativeElement.querySelector('.map-container') as HTMLDivElement;
    if (!container) return;

    (container as any).style.height = `${this.height()}px`;

    this.map = L.map(container, {
      center: [this.centerLat(), this.centerLon()],
      zoom: 13,
      zoomControl: true
    });

    this.tileLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
      maxZoom: 19
    }).addTo(this.map);

    if (this.mode() === 'polygon') {
      this.initPolygonMode();
    } else {
      this.initPointMode();
    }

    // After map is ready, load any pending data from parent
    setTimeout(() => {
      this.map?.invalidateSize();
      const verts = this.vertices();
      if (this.mode() === 'polygon' && verts.length > 0) {
        this.loadVerticesIntoMap(verts);
      }
      // Draw any geofence overlays that were set while map was initializing
      const overlays = this.geofenceOverlays();
      if (overlays.length > 0) {
        this.drawGeofenceOverlays(overlays);
      }
    }, 200);
  }

  // ─── Point mode ───────────────────────────────────────

  private initPointMode() {
    this.pointMarker = L.marker([this.centerLat(), this.centerLon()], {
      draggable: this.editable()
    }).addTo(this.map);

    if (this.editable()) {
      this.map.on('click', (e: L.LeafletMouseEvent) => {
        this.pointMarker.setLatLng([e.latlng.lat, e.latlng.lng]);
        this.centerChanged.emit({ lat: e.latlng.lat, lon: e.latlng.lng });
      });

      this.pointMarker.on('dragend', () => {
        const pos = this.pointMarker.getLatLng();
        this.centerChanged.emit({ lat: pos.lat, lon: pos.lng });
      });

      this.pointMarker.bindTooltip('Кликните по карте или перетащите метку', {
        direction: 'top', offset: L.point(0, -10)
      });
    }
  }

  // ─── Polygon mode ─────────────────────────────────────

  private initPolygonMode() {
    this.polygonLine = L.polyline([], { color: '#1976d2', weight: 2, dashArray: '6 4' }).addTo(this.map);
    this.polygonFill = L.polygon([], {
      color: '#1976d2', fillColor: '#1976d2', fillOpacity: 0.15, weight: 2
    }).addTo(this.map);

    if (this.editable()) {
      this.map.on('click', (e: L.LeafletMouseEvent) => {
        this.addVertex(e.latlng.lat, e.latlng.lng);
        this.emitVertices();
      });
    }
  }

  private addVertex(lat: number, lon: number) {
    const marker = L.marker([lat, lon], { draggable: true })
      .addTo(this.map)
      .bindTooltip(`${this.vertexMarkers.length + 1}`, { direction: 'top', permanent: true, offset: L.point(0, -8) });

    marker.on('dragend', () => {
      this.redrawPolygon();
      this.emitVertices();
    });

    this.vertexMarkers.push(marker);
    this.redrawPolygon();
  }

  private redrawPolygon() {
    const pts = this.vertexMarkers.map(m => m.getLatLng());
    this.polygonLine.setLatLngs(pts);
    if (pts.length >= 3) {
      this.polygonFill.setLatLngs(pts);
      this.polygonFill.setStyle({ fillOpacity: 0.15 });
    } else {
      this.polygonFill.setLatLngs([]);
    }
  }

  clearPolygon() {
    this.vertexMarkers.forEach(m => this.map.removeLayer(m));
    this.vertexMarkers = [];
    this.polygonLine.setLatLngs([]);
    this.polygonFill.setLatLngs([]);
    this.emitVertices();
  }

  private emitVertices() {
    const pts = this.vertexMarkers.map(m => {
      const ll = m.getLatLng();
      return { lat: ll.lat, lon: ll.lng };
    });
    this.verticesChanged.emit(pts);
  }

  // ─── Public API ───────────────────────────────────────

  /** Draw geofence polygons as colored overlays on the map */
  drawGeofenceOverlays(geofences: { name: string; type: string; vertices: { lat: number; lon: number }[] }[]) {
    this.clearGeofenceOverlays();
    if (!this.map || geofences.length === 0) return;

    const colors: Record<string, string> = {
      Depot: '#4caf50',
      DeliveryZone: '#2196f3',
      Restricted: '#ff9800',
      NoGo: '#f44336'
    };

    for (const gf of geofences) {
      if (gf.vertices.length < 3) continue;
      const latlngs = gf.vertices.map(v => L.latLng(v.lat, v.lon));
      const color = colors[gf.type] || '#9e9e9e';
      const poly = L.polygon(latlngs, {
        color,
        fillColor: color,
        fillOpacity: 0.12,
        weight: 2,
        dashArray: gf.type === 'Restricted' || gf.type === 'NoGo' ? '5 5' : undefined
      }).addTo(this.map);
      poly.bindTooltip(`${gf.type}: ${gf.name}`, { direction: 'center', permanent: true, className: 'gf-tooltip', opacity: 0.85 });
      this.geofenceLayers.push(poly);
    }
  }

  private clearGeofenceOverlays() {
    this.geofenceLayers.forEach(l => this.map?.removeLayer(l));
    this.geofenceLayers = [];
  }

  drawMarkers(markers: { lat: number; lon: number; label: string }[]) {
    this.stopMarkers.forEach(m => this.map?.removeLayer(m));
    this.stopMarkers = [];
    if (!this.map || markers.length === 0) return;

    const bounds = L.latLngBounds([]);
    for (const m of markers) {
      const marker = L.marker([m.lat, m.lon])
        .bindTooltip(m.label, { direction: 'top', offset: [0, -8] })
        .addTo(this.map);
      this.stopMarkers.push(marker);
      bounds.extend([m.lat, m.lon]);
    }
    if (markers.length > 1) {
      this.map.fitBounds(bounds.pad(0.2));
    }
  }

  updatePosition(lat: number, lon: number) {
    if (!this.map) return;
    if (this.mode() === 'point' && this.pointMarker) {
      this.pointMarker.setLatLng([lat, lon]);
    }
    this.map.setView([lat, lon], this.map.getZoom());
  }

  loadVerticesIntoMap(verts: { lat: number; lon: number }[]) {
    this.clearPolygonSilent();
    verts.forEach(v => this.addVertex(v.lat, v.lon));
    this.redrawPolygon();
  }

  private clearPolygonSilent() {
    this.vertexMarkers.forEach(m => this.map?.removeLayer(m));
    this.vertexMarkers = [];
    this.polygonLine?.setLatLngs([]);
    this.polygonFill?.setLatLngs([]);
  }

  ngOnDestroy() {
    this.map?.remove();
  }
}
