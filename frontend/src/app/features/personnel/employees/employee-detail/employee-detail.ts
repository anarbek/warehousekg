import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PersonnelService } from '../../services/personnel.service';
import { PermissionsService } from '../../../../core/services/permissions.service';
import { EmployeeDetailModel } from '../../models/personnel.model';

@Component({
  selector: 'app-employee-detail',
  imports: [DxButtonModule, DxProgressBarModule, RouterLink],
  templateUrl: './employee-detail.html',
  styleUrl: './employee-detail.scss'
})
export class EmployeeDetail implements OnInit {
  private readonly svc = inject(PersonnelService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly perms = inject(PermissionsService);
  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly item = signal<EmployeeDetailModel | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly canDelete = computed(() => this.perms.canDelete('employees'));

  ngOnInit() { this.load(); }
  private load() {
    this.loading.set(true); this.error.set(null);
    this.svc.getEmployeeDetail(this.id).subscribe({
      next: d => { this.item.set(d); this.loading.set(false); },
      error: () => { this.error.set('Load error'); this.loading.set(false); }
    });
  }

  protected delete() {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.svc.deleteEmployee(this.id).subscribe({ next: () => void this.router.navigate(['../..'], { relativeTo: this.route }) });
  }
}
