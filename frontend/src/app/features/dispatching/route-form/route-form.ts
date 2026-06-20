import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DxButtonModule, DxProgressBarModule, DxDateBoxModule, DxSelectBoxModule, DxTextBoxModule, DxTextAreaModule } from 'devextreme-angular';
import { firstValueFrom } from 'rxjs';
import { DispatchingService } from '../services/dispatching.service';
import { VehiclesService } from '../../vehicles/services/vehicles.service';
import { PersonnelService } from '../../personnel/services/personnel.service';
import { Vehicle } from '../../vehicles/models/vehicles.model';
import { Employee } from '../../personnel/models/personnel.model';

@Component({
  selector: 'app-route-form',
  imports: [DxButtonModule, DxProgressBarModule, DxDateBoxModule, DxSelectBoxModule, DxTextBoxModule, DxTextAreaModule, RouterLink],
  templateUrl: './route-form.html',
  styleUrl: './route-form.scss'
})
export class RouteForm implements OnInit {
  private readonly svc = inject(DispatchingService);
  private readonly vehiclesSvc = inject(VehiclesService);
  private readonly personnelSvc = inject(PersonnelService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected editId: string | null = null;

  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly routeStatus = signal<string>('Planned');
  protected readonly isEditable = computed(() => {
    const s = this.routeStatus();
    return s === 'Planned' || s === 'InProgress';
  });

  protected readonly vehicles = signal<Vehicle[]>([]);
  protected readonly drivers = signal<Employee[]>([]);
  protected readonly loadingDrivers = signal(false);

  protected readonly form = signal({
    code: '',
    date: new Date().toISOString().slice(0, 16),
    vehicleId: null as string | null,
    driverEmployeeId: null as string | null,
    notes: '' as string
  });

  async ngOnInit() {
    // Load vehicles for dropdown
    try {
      this.vehicles.set(await firstValueFrom(this.vehiclesSvc.getVehicles()));
    } catch { /* ignore */ }

    this.editId = this.route.snapshot.paramMap.get('id');
    if (this.editId) {
      this.loading.set(true);
      this.svc.getRouteById(this.editId).subscribe({
        next: async r => {
          this.routeStatus.set(r.status);
          this.form.set({
            code: r.code,
            date: r.date ? r.date.slice(0, 16) : '',
            vehicleId: r.vehicleId ?? null,
            driverEmployeeId: r.driverEmployeeId ?? null,
            notes: r.notes ?? ''
          });
          // Load drivers for the existing vehicle
          if (r.vehicleId) {
            await this.loadDriversForVehicle(r.vehicleId);
          }
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
    }
  }

  protected async onVehicleChanged(vehicleId: string | null) {
    this.form.update(f => ({ ...f, vehicleId, driverEmployeeId: null }));
    this.drivers.set([]);
    if (vehicleId) {
      await this.loadDriversForVehicle(vehicleId);
    }
  }

  private async loadDriversForVehicle(vehicleId: string) {
    this.loadingDrivers.set(true);
    try {
      const routeDate = new Date(this.form().date);
      // Get all employees assigned to this vehicle
      const assignments = await firstValueFrom(this.vehiclesSvc.getAssignmentsByVehicle(vehicleId));
      if (assignments.length === 0) {
        this.drivers.set([]);
        return;
      }
      // Filter: assignment must be active on the route date
      const activeDriverIds = assignments
        .filter(a => {
          const from = new Date(a.assignedFromUtc);
          const to = a.assignedToUtc ? new Date(a.assignedToUtc) : null;
          return routeDate >= from && (!to || routeDate <= to);
        })
        .map(a => a.employeeId);

      if (activeDriverIds.length > 0) {
        // Fetch employee details
        const allEmployees = await firstValueFrom(this.personnelSvc.getEmployees());
        const filtered = allEmployees.filter(e => activeDriverIds.includes(e.id));
        this.drivers.set(filtered);
      } else {
        this.drivers.set([]);
      }
    } catch {
      this.drivers.set([]);
    } finally {
      this.loadingDrivers.set(false);
    }
  }

  protected async save() {
    this.saving.set(true);
    const body = {
      code: this.form().code,
      date: new Date(this.form().date).toISOString(),
      vehicleId: this.form().vehicleId || undefined,
      driverEmployeeId: this.form().driverEmployeeId || undefined,
      notes: this.form().notes || undefined
    };

    try {
      if (this.editId) {
        await firstValueFrom(this.svc.updateRoute(this.editId, body));
        this.router.navigate(['/dispatching/routes', this.editId]);
      } else {
        const id = await firstValueFrom(this.svc.createRoute(body as any));
        this.router.navigate(['/dispatching/routes', id]);
      }
    } catch {
      /* error */
    } finally {
      this.saving.set(false);
    }
  }
}
