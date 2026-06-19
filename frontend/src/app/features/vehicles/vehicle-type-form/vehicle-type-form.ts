import { Component, inject, signal, OnInit } from '@angular/core';
import { DxFormModule, DxButtonModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { VehiclesService } from '../services/vehicles.service';
import { CreateVehicleTypeRequest, UpdateVehicleTypeRequest, VehicleType } from '../models/vehicles.model';

@Component({
  selector: 'app-vehicle-type-form',
  imports: [DxFormModule, DxButtonModule, RouterLink],
  templateUrl: './vehicle-type-form.html',
  styleUrl: './vehicle-type-form.scss'
})
export class VehicleTypeForm implements OnInit {
  private readonly svc = inject(VehiclesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = this.editId !== null;
  protected readonly loading = signal(false);
  protected formData: any = { code: '', name: '', description: '', defaultCapacityKg: null, defaultCapacityM3: null, isActive: true };

  ngOnInit() {
    if (this.isEdit && this.editId) {
      this.loading.set(true);
      this.svc.getVehicleTypeById(this.editId).subscribe({
        next: d => { this.formData = d; this.loading.set(false); },
        error: () => this.loading.set(false)
      });
    }
  }

  protected submit() {
    this.loading.set(true);
    if (this.isEdit && this.editId) {
      const r: UpdateVehicleTypeRequest = { ...this.formData };
      this.svc.updateVehicleType(this.editId, r).subscribe({
        next: () => { this.loading.set(false); this.router.navigate(['/vehicles/types']); },
        error: () => this.loading.set(false)
      });
    } else {
      const r: CreateVehicleTypeRequest = { ...this.formData };
      this.svc.createVehicleType(r).subscribe({
        next: () => { this.loading.set(false); this.router.navigate(['/vehicles/types']); },
        error: () => this.loading.set(false)
      });
    }
  }
}
