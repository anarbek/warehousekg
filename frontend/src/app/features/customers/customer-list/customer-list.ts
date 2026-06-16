import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { Customer } from '../models/customer-so.model';
import { CustomerSoService } from '../services/customer-so.service';

@Component({
  selector: 'app-customer-list',
  imports: [MatTableModule, MatPaginatorModule, MatSortModule, MatButtonModule, MatIconModule, MatTooltipModule, MatProgressBarModule, RouterLink],
  templateUrl: './customer-list.html',
  styleUrl: './customer-list.scss',
})
export class CustomerList implements OnInit, AfterViewInit {
  private readonly svc = inject(CustomerSoService);
  protected readonly displayedColumns = ['code', 'name', 'contactName', 'email', 'isActive', 'actions'];
  protected readonly dataSource = new MatTableDataSource<Customer>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  ngOnInit(): void { this.load(); }
  ngAfterViewInit(): void { this.dataSource.paginator = this.paginator; this.dataSource.sort = this.sort; }
  protected load(): void { this.loading.set(true); this.error.set(null); this.svc.getCustomers().subscribe({ next: d => { this.dataSource.data = d; this.loading.set(false); }, error: () => { this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`); this.loading.set(false); } }); }
  protected del(id: string): void { if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return; this.svc.deleteCustomer(id).subscribe({ next: () => this.load() }); }
}
