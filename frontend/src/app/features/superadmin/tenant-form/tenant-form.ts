import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DxFormModule, DxButtonModule, DxTagBoxModule } from 'devextreme-angular';
import { TenantService } from '../tenant.service';
import { AppSettings } from '../../../core/config/app-settings';

const AVAILABLE_MODULES = [
  { id: 'inventory', text: 'Складской учёт' },
  { id: 'crm', text: 'CRM (клиенты/поставщики)' },
  { id: 'dispatching', text: 'Диспетчерская' },
  { id: 'personnel', text: 'Персонал' },
  { id: 'vehicles', text: 'Автопарк' },
  { id: 'reports', text: 'Отчёты' },
  { id: 'audits', text: 'Аудиты' },
];

@Component({
  selector: 'app-tenant-form',
  imports: [DxFormModule, DxButtonModule, DxTagBoxModule],
  templateUrl: './tenant-form.html',
  styleUrl: './tenant-form.scss',
})
export class TenantForm {
  private readonly svc = inject(TenantService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = this.editId !== null;
  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly err = signal<string | null>(null);

  protected formData: any = {
    name: '',
    slug: '',
    contactEmail: '',
    contactPhone: '',
    defaultCurrency: 'KGS',
    adminUserName: '',
    adminEmail: '',
    adminPassword: '',
    seedDemoData: false,
    maxUsers: 10,
    enabledModules: [] as string[],
  };

  protected readonly createFormItems: any[] = [
    { itemType: 'group', caption: 'Организация', colCount: 2, items: [
      { dataField: 'name', label: { text: 'Название' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'slug', label: { text: 'Slug' }, isRequired: true, editorOptions: { placeholder: 'my-company', stylingMode: 'outlined' } },
      { dataField: 'contactEmail', label: { text: 'Email' }, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'contactPhone', label: { text: 'Телефон' }, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'defaultCurrency', label: { text: 'Валюта' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
    ]},
    { itemType: 'group', caption: 'Администратор арендатора', colCount: 2, items: [
      { dataField: 'adminUserName', label: { text: 'Логин администратора' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'adminEmail', label: { text: 'Email администратора' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'adminPassword', label: { text: 'Пароль администратора' }, isRequired: true, editorOptions: { mode: 'password', stylingMode: 'outlined' } },
    ]},
    { itemType: 'group', caption: 'Дополнительно', colCount: 2, items: [
      { dataField: 'seedDemoData', editorType: 'dxCheckBox', label: { text: 'Заполнить демонстрационными данными' } },
    ]},
  ];

  protected readonly editFormItems: any[] = [
    { itemType: 'group', caption: 'Организация', colCount: 2, items: [
      { dataField: 'name', label: { text: 'Название' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'slug', label: { text: 'Slug' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'contactEmail', label: { text: 'Email' }, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'contactPhone', label: { text: 'Телефон' }, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'defaultCurrency', label: { text: 'Валюта' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
      { dataField: 'maxUsers', label: { text: 'Макс. пользователей' }, editorType: 'dxNumberBox', editorOptions: { min: 1, stylingMode: 'outlined' } },
    ]},
    { itemType: 'group', caption: 'Модули', colCount: 1, items: [
      { dataField: 'enabledModules', label: { text: 'Включённые модули' },
        editorType: 'dxTagBox',
        editorOptions: {
          items: AVAILABLE_MODULES,
          displayExpr: 'text',
          valueExpr: 'id',
          stylingMode: 'outlined',
          placeholder: 'Выберите модули...',
          showSelectionControls: true,
        },
      },
    ]},
  ];

  constructor() {
    if (this.isEdit) {
      this.loadTenant();
    }
  }

  private loadTenant(): void {
    this.loading.set(true);
    this.svc.getTenantById(this.editId!).subscribe({
      next: (data) => {
        this.formData = {
          name: data.name,
          slug: data.slug,
          contactEmail: data.contactEmail ?? '',
          contactPhone: data.contactPhone ?? '',
          defaultCurrency: data.defaultCurrency,
          maxUsers: data.maxUsers,
          enabledModules: data.enabledModules
            ? data.enabledModules.split(',').map((m: string) => m.trim()).filter(Boolean)
            : [],
        };
        this.loading.set(false);
      },
      error: () => {
        this.err.set($localize`:@@tenant.form.loadError:Ошибка загрузки арендатора`);
        this.loading.set(false);
      },
    });
  }

  protected get formItems(): any[] {
    return this.isEdit ? this.editFormItems : this.createFormItems;
  }

  protected get title(): string {
    return this.isEdit
      ? $localize`:@@tenant.form.editTitle:Редактирование арендатора`
      : $localize`:@@tenant.form.createTitle:Новый арендатор`;
  }

  submit(): void {
    this.saving.set(true);
    this.err.set(null);

    if (this.isEdit) {
      const request = {
        name: this.formData.name,
        slug: this.formData.slug,
        contactEmail: this.formData.contactEmail || null,
        contactPhone: this.formData.contactPhone || null,
        defaultCurrency: this.formData.defaultCurrency,
        maxUsers: this.formData.maxUsers,
        enabledModules: Array.isArray(this.formData.enabledModules)
          ? this.formData.enabledModules.join(',') || null
          : this.formData.enabledModules || null,
      };
      this.svc.updateTenant(this.editId!, request).subscribe({
        next: () => { this.saving.set(false); this.cancel(); },
        error: (e) => { this.err.set(e?.error?.title ?? $localize`:@@tenant.form.saveError:Ошибка сохранения`); this.saving.set(false); },
      });
    } else {
      const request = {
        name: this.formData.name,
        slug: this.formData.slug,
        contactEmail: this.formData.contactEmail || null,
        contactPhone: this.formData.contactPhone || null,
        defaultCurrency: this.formData.defaultCurrency,
        adminUserName: this.formData.adminUserName,
        adminEmail: this.formData.adminEmail,
        adminPassword: this.formData.adminPassword,
        seedDemoData: this.formData.seedDemoData ?? false,
      };
      this.svc.createTenant(request).subscribe({
        next: () => { this.saving.set(false); this.cancel(); },
        error: (e) => { this.err.set(e?.error?.title ?? $localize`:@@tenant.form.saveError:Ошибка сохранения`); this.saving.set(false); },
      });
    }
  }

  cancel(): void {
    void this.router.navigate(['..'], { relativeTo: this.route });
  }
}
