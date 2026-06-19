import { Component, OnInit, inject, signal } from '@angular/core';
import { DxFormModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { PersonnelService } from '../../services/personnel.service';
import { ErrorToastService } from '../../../../core/services/error-toast.service';
import { Employee } from '../../models/personnel.model';
import { Shift } from '../../models/personnel.model';

@Component({
  selector: 'app-attendance-form',
  imports: [DxFormModule, DxButtonModule, DxProgressBarModule],
  templateUrl: './attendance-form.html',
  styleUrl: './attendance-form.scss'
})
export class AttendanceForm implements OnInit {
  private readonly svc = inject(PersonnelService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ErrorToastService);
  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = this.editId !== null;
  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly title = this.isEdit ? 'Редактировать запись' : 'Новая запись';

  protected readonly employees = signal<Employee[]>([]);
  protected readonly shifts = signal<Shift[]>([]);
  protected readonly statuses = [
    { value: 'Present', text: 'Присутствует' },
    { value: 'Absent', text: 'Отсутствует' },
    { value: 'Late', text: 'Опоздание' },
    { value: 'EarlyDeparture', text: 'Ранний уход' },
    { value: 'Overtime', text: 'Сверхурочно' },
  ];

  protected formData: any = {
    employeeId: null, shiftId: null, date: new Date(), clockInUtc: null, clockOutUtc: null,
    status: 'Present', notes: ''
  };

  protected get formItems(): any[] {
    return [
      { dataField: 'employeeId', editorType: 'dxSelectBox', isRequired: true, label: { text: 'Сотрудник' },
        editorOptions: { dataSource: this.employees(), displayExpr: 'lastName', valueExpr: 'id', placeholder: 'Сотрудник', stylingMode: 'outlined' } },
      { dataField: 'shiftId', editorType: 'dxSelectBox', isRequired: true, label: { text: 'Смена' },
        editorOptions: { dataSource: this.shifts(), displayExpr: 'name', valueExpr: 'id', placeholder: 'Смена', stylingMode: 'outlined' } },
      { dataField: 'date', editorType: 'dxDateBox', isRequired: true, label: { text: 'Дата' } },
      { dataField: 'status', editorType: 'dxSelectBox', isRequired: true, label: { text: 'Статус' },
        editorOptions: { dataSource: this.statuses, displayExpr: 'text', valueExpr: 'value', stylingMode: 'outlined' } },
      { dataField: 'clockInUtc', editorType: 'dxDateBox', label: { text: 'Приход' }, editorOptions: { type: 'datetime' } },
      { dataField: 'clockOutUtc', editorType: 'dxDateBox', label: { text: 'Уход' }, editorOptions: { type: 'datetime' } },
      { dataField: 'notes', editorType: 'dxTextArea', label: { text: 'Примечание' }, editorOptions: { stylingMode: 'outlined' } },
    ];
  }

  ngOnInit() {
    this.svc.getEmployees().subscribe({ next: d => this.employees.set(d) });
    this.svc.getShifts().subscribe({ next: d => this.shifts.set(d) });
    if (!this.isEdit) return;
    this.loading.set(true);
    this.svc.getAttendanceById(this.editId!).subscribe({
      next: s => { this.formData = s; this.loading.set(false); },
      error: e => { this.toast.showLoad(e); this.loading.set(false); }
    });
  }

  submit() {
    this.saving.set(true);
    const r: any = { ...this.formData };
    const done = () => { this.saving.set(false); void this.router.navigate(['..'], { relativeTo: this.route }); };
    const fail = (e: any) => { this.toast.showSave(e); this.saving.set(false); };
    if (this.isEdit) {
      (this.svc.updateAttendance(this.editId!, r) as any).subscribe({ next: done, error: fail });
    } else {
      (this.svc.createAttendance(r) as any).subscribe({ next: done, error: fail });
    }
  }

  cancel() { void this.router.navigate(['..'], { relativeTo: this.route }); }
}
