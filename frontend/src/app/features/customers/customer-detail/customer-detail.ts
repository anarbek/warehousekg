import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DxButtonModule, DxToolbarModule } from 'devextreme-angular';
import { CustomerSoService } from '../services/customer-so.service';
import { ErrorToastService } from '../../../core/services/error-toast.service';
import { Customer } from '../models/customer-so.model';
import { MapPicker } from '../../../shared/map-picker/map-picker';

@Component({
  selector: 'app-customer-detail',
  imports: [DxButtonModule, DxToolbarModule, RouterLink, MapPicker],
  templateUrl: './customer-detail.html',
  styleUrl: './customer-detail.scss',
})
export class CustomerDetail implements OnInit {
  private readonly svc = inject(CustomerSoService);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ErrorToastService);

  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly customer = signal<Customer | null>(null);
  protected readonly loading = signal(false);

  ngOnInit() {
    this.loading.set(true);
    this.svc.getCustomerById(this.id).subscribe({
      next: (c) => { this.customer.set(c); this.loading.set(false); },
      error: (e) => { this.toast.showLoad(e); this.loading.set(false); },
    });
  }
}
