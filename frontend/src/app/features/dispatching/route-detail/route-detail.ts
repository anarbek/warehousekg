import { Component, inject, signal, computed, viewChild, OnInit, NgZone } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxDateBoxModule, DxNumberBoxModule, DxTextBoxModule, DxTextAreaModule } from 'devextreme-angular';
import { firstValueFrom, timeout } from 'rxjs';
import { AppSettings } from '../../../core/config/app-settings';
import { MapPicker } from '../../../shared/map-picker/map-picker';
import { DispatchingService } from '../services/dispatching.service';
import { DeliveryRoute, DeliveryRouteDetail, DeliveryStop, DeliveryShipment, GeofenceCheckResult, UnassignedSalesOrder } from '../models/dispatching.model';
import { PermissionsService } from '../../../core/services/permissions.service';

@Component({
  selector: 'app-route-detail',
  imports: [
    DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule,
    DxSelectBoxModule, DxDateBoxModule, DxNumberBoxModule, DxTextBoxModule, DxTextAreaModule,
    RouterLink, DatePipe, DecimalPipe, MapPicker
  ],
  templateUrl: './route-detail.html',
  styleUrl: './route-detail.scss'
})
export class RouteDetail implements OnInit {
  private readonly svc = inject(DispatchingService);
  private readonly http = inject(HttpClient);
  private readonly zone = inject(NgZone);
  private readonly activatedRoute = inject(ActivatedRoute);
  readonly perms = inject(PermissionsService);
  protected readonly route = signal<DeliveryRouteDetail | null>(null);
  protected readonly loading = signal(false);
  protected readonly activeTab = signal<'stops' | 'info'>('stops');
  protected readonly canWrite = computed(() => this.perms.canWrite('delivery-routes'));
  protected readonly canWriteStops = computed(() => this.perms.canWrite('delivery-stops'));
  protected readonly isEditable = computed(() => {
    const s = this.route()?.status;
    return s === 'Planned' || s === 'InProgress';
  });

  // Customers for stop form dropdown
  protected readonly customers = signal<{ id: string; name: string }[]>([]);

  readonly mapPicker = viewChild(MapPicker);

  // Geofence check results
  protected readonly geofenceResults = signal<GeofenceCheckResult[]>([]);
  protected readonly checkingGeofence = signal(false);

  // Geofence overlays for the stop map
  protected readonly geofenceOverlays = signal<{ name: string; type: string; vertices: { lat: number; lon: number }[] }[]>([]);

  // Stop popup
  protected readonly stopPopup = signal(false);
  protected readonly editingStopId = signal<string | null>(null);
  protected stopForm: any = {
    sequenceNumber: 1, customerId: null, address: '',
    latitude: null, longitude: null,
    plannedArrivalUtc: null, plannedDepartureUtc: null, notes: ''
  };

  // Shipment popup
  protected readonly shipmentPopup = signal(false);
  protected readonly currentStopForShipment = signal<string | null>(null);
  protected shipmentForm: any = { salesOrderId: null };

  // Shipments for current stop
  protected readonly currentShipments = signal<DeliveryShipment[]>([]);
  protected readonly loadingShipments = signal(false);

  // Unassigned sales orders for dropdown (plain array for DevExtreme compatibility)
  protected unassignedOrders: UnassignedSalesOrder[] = [];

  // Stop markers for the route map
  protected readonly stopMarkers = computed(() => {
    const stops = this.route()?.stops ?? [];
    return stops
      .filter(s => s.latitude && s.longitude)
      .map(s => ({ lat: s.latitude!, lon: s.longitude!, label: `${s.customerName || s.address} (#${s.sequenceNumber})` }));
  });

  protected routeId = '';

  ngOnInit() {
    this.routeId = this.activatedRoute.snapshot.paramMap.get('id') || '';
    if (this.routeId) this.load(this.routeId);
  }

  load(id: string) {
    this.routeId = id;
    this.loading.set(true);
    this.svc.getRouteDetail(id).subscribe({
      next: d => { this.route.set(d); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    // Preload unassigned sales orders for shipment dropdown
    this.svc.getUnassignedSalesOrders().subscribe({
      next: o => { this.unassignedOrders = o; },
      error: () => {}
    });
  }

  protected statusLabel(s: string) {
    const map: Record<string, string> = {
      Planned: 'Запланирован', InProgress: 'В пути', Completed: 'Завершён', Cancelled: 'Отменён',
      Pending: 'Ожидает', Skipped: 'Пропущен'
    };
    return map[s] ?? s;
  }

  protected statusClass(s: string): string {
    const map: Record<string, string> = {
      Planned: 'status--planned', InProgress: 'status--active', Completed: 'status--completed',
      Cancelled: 'status--cancelled', Pending: 'status--planned', Skipped: 'status--cancelled'
    };
    return map[s] ?? '';
  }

  // ─── Route actions ─────────────────────────────────
  protected async startRoute() { try { await firstValueFrom(this.svc.startRoute(this.routeId).pipe(timeout(15000))); this.load(this.routeId); } catch { } }
  protected async completeRoute() {
    if (!confirm('Завершить маршрут?')) return;
    try { await firstValueFrom(this.svc.completeRoute(this.routeId).pipe(timeout(15000))); this.load(this.routeId); } catch { }
  }
  protected async cancelRoute() {
    if (!confirm('Отменить маршрут?')) return;
    try { await firstValueFrom(this.svc.cancelRoute(this.routeId).pipe(timeout(15000))); this.load(this.routeId); } catch { }
  }
  protected canStart() { return this.route()?.status === 'Planned'; }
  protected canComplete() { return this.route()?.status === 'InProgress'; }
  protected canCancel() { return this.route()?.status === 'Planned' || this.route()?.status === 'InProgress'; }

  protected printManifest() {
    window.print();
  }

  // ─── Stop popup ────────────────────────────────────
  protected async openStopPopup(stop?: DeliveryStop) {
    if (stop) {
      this.editingStopId.set(stop.id);
      this.stopForm = { ...stop, customerId: stop.customerId ?? null };
    } else {
      this.editingStopId.set(null);
      const maxSeq = this.route()?.stops?.length ?? 0;
      this.stopForm = {
        sequenceNumber: maxSeq + 1, customerId: null, address: '',
        latitude: 42.8746, longitude: 74.5698,
        plannedArrivalUtc: null, plannedDepartureUtc: null, notes: ''
      };
    }

    // Fetch customers for dropdown
    if (this.customers().length === 0) {
      try {
        const data = await firstValueFrom(
          this.http.get<{ id: string; name: string }[]>(`${AppSettings.apiBaseUrl}/customers`).pipe(timeout(5000))
        );
        this.customers.set(data);
      } catch { }
    }
    this.stopPopup.set(true);

    // Load geofence overlays for the map
    try {
      const fences = await firstValueFrom(this.svc.getGeofences().pipe(timeout(5000)));
      this.geofenceOverlays.set(
        fences.filter(f => f.isActive && f.vertices?.length >= 3).map(f => ({
          name: f.name,
          type: f.type,
          vertices: f.vertices.map((v: any) => ({ lat: v.latitude ?? v.lat, lon: v.longitude ?? v.lon }))
        }))
      );
    } catch {
      this.geofenceOverlays.set([]);
    }

    setTimeout(() => {
      this.mapPicker()?.updatePosition(
        this.stopForm.latitude ?? 42.8746,
        this.stopForm.longitude ?? 74.5698);
    }, 300);
  }
  protected closeStopPopup() { this.stopPopup.set(false); this.editingStopId.set(null); }

  protected onStopMapChanged(e: { lat: number; lon: number }) {
    this.stopForm.latitude = e.lat;
    this.stopForm.longitude = e.lon;
  }

  protected async saveStop() {
    const editId = this.editingStopId();
    try {
      if (editId) {
        await firstValueFrom(
          this.svc.updateStop(this.routeId, editId, this.stopForm).pipe(timeout(15000))
        );
      } else {
        await firstValueFrom(
          this.svc.createStop(this.routeId, { ...this.stopForm, routeId: this.routeId }).pipe(timeout(15000))
        );
      }
      this.stopPopup.set(false);
      this.load(this.routeId);
    } catch { /* error — timeout or server error */ }
  }

  // ─── Stop workflow ─────────────────────────────────
  protected async arriveStop(stopId: string) {
    try { await firstValueFrom(this.svc.arriveAtStop(this.routeId, stopId).pipe(timeout(15000))); this.load(this.routeId); } catch { }
  }
  protected async completeStop(stopId: string) {
    try { await firstValueFrom(this.svc.completeStop(this.routeId, stopId).pipe(timeout(15000))); this.load(this.routeId); } catch { }
  }
  protected async skipStop(stopId: string) {
    if (!confirm('Пропустить остановку?')) return;
    try { await firstValueFrom(this.svc.skipStop(this.routeId, stopId).pipe(timeout(15000))); this.load(this.routeId); } catch { }
  }
  protected async deleteStop(stopId: string) {
    if (!confirm('Удалить остановку?')) return;
    try { await firstValueFrom(this.svc.deleteStop(this.routeId, stopId).pipe(timeout(15000))); this.load(this.routeId); } catch { }
  }

  protected canArrive(s: string) { return s === 'Pending'; }
  protected canCompleteStop(s: string) { return s === 'InProgress'; }
  protected canSkip(s: string) { return s === 'Pending' || s === 'InProgress'; }

  // ─── Geofence check ────────────────────────────────
  protected async checkGeofence(stopId: string) {
    this.checkingGeofence.set(true);
    try {
      const r = await firstValueFrom(this.svc.checkStopAgainstGeofences(stopId).pipe(timeout(15000)));
      this.geofenceResults.set(r);
    } catch { }
    this.checkingGeofence.set(false);
  }

  // ─── Shipments ─────────────────────────────────────
  protected async openShipments(stopId: string) {
    this.currentStopForShipment.set(stopId);
    this.loadingShipments.set(true);
    this.shipmentForm.salesOrderId = null;
    this.shipmentPopup.set(true);
    // Fetch shipments for this stop
    try {
      const s = await firstValueFrom(this.svc.getShipmentsByStop(stopId).pipe(timeout(15000)));
      this.zone.run(() => this.currentShipments.set(s));
    } catch { }
    this.loadingShipments.set(false);
  }
  protected closeShipments() { this.shipmentPopup.set(false); this.currentStopForShipment.set(null); }

  protected async assignShipment() {
    const stopId = this.currentStopForShipment();
    if (!stopId) return;
    try {
      await firstValueFrom(this.svc.assignShipment(stopId, this.shipmentForm).pipe(timeout(15000)));
      this.openShipments(stopId);
      this.load(this.routeId);
    } catch { }
  }
  protected async removeShipment(shipmentId: string) {
    if (!confirm('Убрать заказ из доставки?')) return;
    try {
      await firstValueFrom(this.svc.removeShipment(shipmentId).pipe(timeout(15000)));
      const stopId = this.currentStopForShipment();
      if (stopId) { this.openShipments(stopId); this.load(this.routeId); }
    } catch { }
  }

  protected hasRegulatedLabel(has: boolean): string {
    return has ? '⚠ Требуется проверка возраста' : '';
  }

  protected typeLabel(t: string): string {
    const map: Record<string, string> = { Depot: 'Склад', DeliveryZone: 'Зона доставки', Restricted: 'Ограниченная', NoGo: 'Запрещённая' };
    return map[t] ?? t;
  }

  protected geofenceResultClass(r: GeofenceCheckResult): string {
    if (!r.isInside) return 'geofence-result outside';
    switch (r.type) {
      case 'NoGo': return 'geofence-result danger';
      case 'Restricted': return 'geofence-result warning';
      default: return 'geofence-result ok';
    }
  }

  protected geofenceResultText(r: GeofenceCheckResult): string {
    if (!r.isInside) return '✕ Снаружи';
    switch (r.type) {
      case 'NoGo': return '⚠ Нарушение! Запрещённая зона';
      case 'Restricted': return '⚠ Требуется разрешение';
      default: return '✓ Внутри (норма)';
    }
  }
}
