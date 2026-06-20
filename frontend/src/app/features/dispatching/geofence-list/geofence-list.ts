import { Component, inject, signal, computed, viewChild } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxNumberBoxModule, DxTextBoxModule } from 'devextreme-angular';
import { FormsModule } from '@angular/forms';
import { firstValueFrom, timeout } from 'rxjs';
import { MapPicker } from '../../../shared/map-picker/map-picker';
import { DispatchingService } from '../services/dispatching.service';
import { Geofence } from '../models/dispatching.model';
import { PermissionsService } from '../../../core/services/permissions.service';

@Component({
  selector: 'app-geofence-list',
  imports: [DxDataGridModule, DxButtonModule, DxProgressBarModule, DxPopupModule, DxSelectBoxModule, DxNumberBoxModule, DxTextBoxModule, FormsModule, MapPicker],
  templateUrl: './geofence-list.html',
  styleUrl: './geofence-list.scss'
})
export class GeofenceList {
  private readonly svc = inject(DispatchingService);
  private readonly perms = inject(PermissionsService);
  protected readonly items = signal<Geofence[]>([]);
  protected readonly loading = signal(false);
  protected readonly canWrite = computed(() => this.perms.canWrite('geofences'));
  protected readonly canDelete = computed(() => this.perms.canDelete('geofences'));

  readonly mapPicker = viewChild(MapPicker);

  protected readonly popup = signal(false);
  protected readonly editingId = signal<string | null>(null);
  protected form: any = {
    code: '', name: '', type: 'DeliveryZone',
    vertices: [] as { lat: number; lon: number }[],
    isActive: true
  };
  protected readonly saving = signal(false);
  protected readonly errors = signal<{ code?: string; name?: string; vertices?: string }>({});

  protected readonly typeOptions = [
    { id: 'Depot', name: 'Склад' },
    { id: 'DeliveryZone', name: 'Зона доставки' },
    { id: 'Restricted', name: 'Ограниченная' },
    { id: 'NoGo', name: 'Запрещённая' }
  ];

  constructor() { this.load(); }

  private load() {
    this.loading.set(true);
    this.svc.getGeofences().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  protected typeLabel(t: string): string {
    const map: Record<string, string> = { Depot: 'Склад', DeliveryZone: 'Зона доставки', Restricted: 'Ограниченная', NoGo: 'Запрещённая' };
    return map[t] ?? t;
  }

  protected vertexCount(geofence: Geofence): number {
    return geofence.vertices?.length ?? 0;
  }

  protected onVerticesChanged(pts: { lat: number; lon: number }[]) {
    this.form.vertices = pts;
  }

  protected openPopup(geofence?: Geofence) {
    if (geofence) {
      this.editingId.set(geofence.id);
      this.form = {
        code: geofence.code,
        name: geofence.name,
        type: geofence.type,
        vertices: (geofence.vertices || []).map((v: any) => ({ lat: v.latitude ?? v.lat, lon: v.longitude ?? v.lon })),
        isActive: geofence.isActive
      };
    } else {
      this.editingId.set(null);
      this.form = { code: '', name: '', type: 'DeliveryZone', vertices: [], isActive: true };
    }
    this.popup.set(true);
  }
  protected closePopup() { this.popup.set(false); this.editingId.set(null); }

  protected readonly error = signal<string | null>(null);

  protected validate(): boolean {
    const e: { code?: string; name?: string; vertices?: string } = {};
    if (!this.form.code?.trim()) e.code = 'Код обязателен';
    if (!this.form.name?.trim()) e.name = 'Название обязательно';
    if (this.form.vertices.length < 3) e.vertices = 'Минимум 3 вершины';
    this.errors.set(e);
    return Object.keys(e).length === 0;
  }

  protected async save() {
    this.error.set(null);
    if (!this.validate()) return;

    this.saving.set(true);
    const vertices = this.form.vertices.map((v: any) => ({
      latitude: v.lat ?? v.latitude,
      longitude: v.lon ?? v.longitude
    }));
    const body: any = {
      code: this.form.code,
      name: this.form.name,
      type: this.form.type,
      vertices,
      isActive: this.form.isActive
    };

    const id = this.editingId();
    try {
      if (id) {
        await firstValueFrom(this.svc.updateGeofence(id, body).pipe(timeout(15000)));
      } else {
        await firstValueFrom(this.svc.createGeofence(body).pipe(timeout(15000)));
      }
      this.saving.set(false);
      this.popup.set(false);
      this.load();
    } catch (err: any) {
      this.saving.set(false);
      this.error.set(err?.message || 'Ошибка сохранения');
    }
  }

  protected delete(id: string) {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteGeofence(id).subscribe({ next: () => this.load() });
  }
}
