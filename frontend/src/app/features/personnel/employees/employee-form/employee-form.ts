import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DxFormModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { PersonnelService } from '../../services/personnel.service';
import { ErrorToastService } from '../../../../core/services/error-toast.service';

@Component({
  selector: 'app-employee-form',
  imports: [DxFormModule, DxButtonModule, DxProgressBarModule],
  templateUrl: './employee-form.html',
  styleUrl: './employee-form.scss'
})
export class EmployeeForm implements OnInit {
  private readonly svc = inject(PersonnelService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ErrorToastService);
  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = this.editId !== null;
  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly ready = signal(false);
  protected readonly title = this.isEdit ? 'Редактировать сотрудника' : 'Новый сотрудник';

  protected formData: any = {
    code: '', firstName: '', lastName: '', middleName: '', email: '', phone: '',
    hireDate: null, terminationDate: null, positionId: null, departmentId: null,
    applicationUserId: null, isActive: true
  };

  protected readonly formItems = signal<any[]>([]);

  ngOnInit() {
    forkJoin({
      positions: this.svc.getPositions(),
      departments: this.svc.getDepartments()
    }).subscribe(({ positions, departments }) => {
      const activePositions = positions.filter(p => p.isActive);
      const activeDepartments = departments.filter(d => d.isActive);

      this.formItems.set([
        { dataField: 'code', label: { text: 'Код' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
        { dataField: 'lastName', label: { text: 'Фамилия' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
        { dataField: 'firstName', label: { text: 'Имя' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
        { dataField: 'middleName', label: { text: 'Отчество' }, editorOptions: { stylingMode: 'outlined' } },
        { dataField: 'email', label: { text: 'Email' }, editorOptions: { stylingMode: 'outlined' } },
        { dataField: 'phone', label: { text: 'Телефон' }, editorOptions: { stylingMode: 'outlined' } },
        { dataField: 'positionId', editorType: 'dxSelectBox', label: { text: 'Должность' },
          editorOptions: { dataSource: activePositions, displayExpr: 'name', valueExpr: 'id', placeholder: 'Выберите должность', stylingMode: 'outlined' } },
        { dataField: 'departmentId', editorType: 'dxSelectBox', label: { text: 'Отдел' },
          editorOptions: { dataSource: activeDepartments, displayExpr: 'name', valueExpr: 'id', placeholder: 'Выберите отдел', stylingMode: 'outlined' } },
        { dataField: 'hireDate', editorType: 'dxDateBox', label: { text: 'Дата найма' } },
        { dataField: 'terminationDate', editorType: 'dxDateBox', label: { text: 'Дата увольнения' } },
        { dataField: 'isActive', editorType: 'dxCheckBox', label: { text: 'Активен' } },
      ]);
      this.ready.set(true);

      if (!this.isEdit) return;
      this.loading.set(true);
      this.svc.getEmployeeById(this.editId!).subscribe({
        next: s => { this.formData = { ...s }; this.loading.set(false); },
        error: e => { this.toast.showLoad(e); this.loading.set(false); }
      });
    });
  }

  submit() {
    this.saving.set(true);
    const r: any = { ...this.formData };
    const done = () => { this.saving.set(false); void this.router.navigate(['..'], { relativeTo: this.route }); };
    const fail = (e: any) => { this.toast.showSave(e); this.saving.set(false); };
    if (this.isEdit) {
      (this.svc.updateEmployee(this.editId!, r) as any).subscribe({ next: done, error: fail });
    } else {
      (this.svc.createEmployee(r) as any).subscribe({ next: done, error: fail });
    }
  }

  cancel() { void this.router.navigate(['..'], { relativeTo: this.route }); }
}
