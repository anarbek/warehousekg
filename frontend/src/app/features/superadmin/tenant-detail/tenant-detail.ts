import { Component, inject, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DxFormModule, DxButtonModule, DxToolbarModule, DxPopupModule, DxTextBoxModule } from 'devextreme-angular';
import { TenantService } from '../tenant.service';
import { Tenant, TenantStatus } from '../tenant.model';
import { PermissionsService } from '../../../core/services/permissions.service';

@Component({
  selector: 'app-tenant-detail',
  imports: [DxFormModule, DxButtonModule, DxToolbarModule, DxPopupModule, DxTextBoxModule],
  templateUrl: './tenant-detail.html',
  styleUrl: './tenant-detail.scss',
})
export class TenantDetail {
  private readonly svc = inject(TenantService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly perms = inject(PermissionsService);

  protected readonly tenant = signal<Tenant | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly actionInProgress = signal(false);
  protected readonly TenantStatus = TenantStatus;

  private readonly id = this.route.snapshot.paramMap.get('id')!;

  protected readonly canWrite = computed(() => this.perms.canWrite('tenants'));
  protected readonly canDelete = computed(() => this.perms.canDelete('tenants'));

  // Password reset popup state
  protected readonly passwordResetVisible = signal(false);
  protected readonly newPassword = signal('');
  protected readonly passwordResetError = signal<string | null>(null);
  protected readonly passwordResetting = signal(false);

  constructor() {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.svc.getTenantById(this.id).subscribe({
      next: (data) => { this.tenant.set(data); this.loading.set(false); },
      error: () => { this.error.set($localize`:@@tenant.detail.loadError:Ошибка загрузки`); this.loading.set(false); },
    });
  }

  protected statusText(status: TenantStatus): string {
    switch (status) {
      case TenantStatus.Active: return $localize`:@@tenant.status.active:Активен`;
      case TenantStatus.Suspended: return $localize`:@@tenant.status.suspended:Приостановлен`;
      case TenantStatus.Trial: return $localize`:@@tenant.status.trial:Пробный`;
      default: return '';
    }
  }

  protected suspend(): void {
    this.actionInProgress.set(true);
    this.svc.suspendTenant(this.id).subscribe({
      next: () => { this.actionInProgress.set(false); this.load(); },
      error: () => { this.actionInProgress.set(false); },
    });
  }

  protected activate(): void {
    this.actionInProgress.set(true);
    this.svc.activateTenant(this.id).subscribe({
      next: () => { this.actionInProgress.set(false); this.load(); },
      error: () => { this.actionInProgress.set(false); },
    });
  }

  protected back(): void {
    void this.router.navigate(['/superadmin/tenants']);
  }

  protected edit(): void {
    void this.router.navigate(['/superadmin/tenants', this.id, 'edit']);
  }

  protected openPasswordReset(): void {
    this.newPassword.set('');
    this.passwordResetError.set(null);
    this.passwordResetVisible.set(true);
  }

  protected resetPassword(): void {
    const t = this.tenant();
    if (!t?.adminUserId) return;
    this.passwordResetting.set(true);
    this.passwordResetError.set(null);
    this.svc.resetUserPassword(this.id, t.adminUserId, this.newPassword()).subscribe({
      next: () => {
        this.passwordResetting.set(false);
        this.passwordResetVisible.set(false);
      },
      error: (e) => {
        this.passwordResetError.set(e?.error?.title ?? e?.error ?? 'Ошибка смены пароля');
        this.passwordResetting.set(false);
      },
    });
  }
}
