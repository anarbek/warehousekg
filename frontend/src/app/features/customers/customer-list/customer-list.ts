import { Component, inject, signal } from '@angular/core';
import { DxDataGridModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { RouterLink } from '@angular/router';
import { Customer as CustomerModel } from '../models/customer-so.model';
import { CustomerSoService } from '../services/customer-so.service';

@Component({ selector: 'app-customer-list', imports: [DxDataGridModule,DxButtonModule,DxProgressBarModule,RouterLink], templateUrl: './customer.html', styleUrl: './customer-list.scss' })
export class CustomerList {
  private readonly s = inject(CustomerSoService);
  protected readonly items = signal<CustomerModel[]>([]);
  protected readonly loading = signal(false); protected readonly error = signal<string|null>(null);
  constructor() { this.load(); }
  private load() { this.loading.set(true); this.error.set(null); this.s.getCustomers().subscribe({ next: (d:any) => { this.items.set(d); this.loading.set(false); }, error: () => { this.error.set('Load error'); this.loading.set(false); } }); }
}
