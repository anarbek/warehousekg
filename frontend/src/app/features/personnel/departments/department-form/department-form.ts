import { Component, OnInit, inject, signal } from '@angular/core';
import { DxFormModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { PersonnelService } from '../../services/personnel.service';
import { ErrorToastService } from '../../../../core/services/error-toast.service';

@Component({
  selector: 'app-department-form',
  imports: [DxFormModule, DxButtonModule, DxProgressBarModule],
  templateUrl: './department-form.html',
  styleUrl: './department-form.scss'
})
export class DepartmentForm implements OnInit {
  private readonly svc = inject(PersonnelService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ErrorToastService);
  protected readonly editId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = this.editId !== null;
  protected readonly saving = signal(false);
  protected readonly loading = signal(false);
  protected readonly title = this.isEdit ? 'Редактировать отдел' : 'Новый отдел';
  protected formData: any = { code: '', name: '', description: '', isActive: true };
  protected readonly formItems: any[] = [
    { dataField: 'code', label: { text: 'Код' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
    { dataField: 'name', label: { text: 'Название' }, isRequired: true, editorOptions: { stylingMode: 'outlined' } },
    { dataField: 'description', label: { text: 'Описание' }, editorOptions: { stylingMode: 'outlined' } },
    { dataField: 'isActive', editorType: 'dxCheckBox', label: { text: 'Активен' } },
  ];

  ngOnInit() {
    if (!this.isEdit) return;
    this.loading.set(true);
    this.svc.getDepartmentById(this.editId!).subscribe({
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
      (this.svc.updateDepartment(this.editId!, r) as any).subscribe({ next: done, error: fail });
    } else {
      (this.svc.createDepartment(r) as any).subscribe({ next: done, error: fail });
    }
  }

  cancel() { void this.router.navigate(['..'], { relativeTo: this.route }); }
}
