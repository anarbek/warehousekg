import { Component, OnInit, inject, signal } from '@angular/core';
import { DxFormModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { CreateWarehouseRequest } from '../../models/warehouse.model';
import { InventoryService } from '../../services/inventory.service';
import { ErrorToastService } from '../../../../core/services/error-toast.service';

@Component({selector:'app-warehouse-form',imports:[DxFormModule,DxButtonModule,DxProgressBarModule],templateUrl:'./warehouse-form.html',styleUrl:'./warehouse-form.scss'})
export class WarehouseForm implements OnInit {
  private readonly svc=inject(InventoryService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);private readonly toast=inject(ErrorToastService);
  protected readonly editId=this.route.snapshot.paramMap.get('id');protected readonly isEdit=this.editId!==null;
  protected readonly saving=signal(false);protected readonly loading=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly title=this.isEdit?$localize`:@@warehouse.form.edit.title:Редактировать склад`:$localize`:@@warehouse.form.create.title:Новый склад`;
  protected formData:any={code:'',name:'',address:'',isActive:true};
  protected readonly formItems:any[]=[
    {dataField:'code',label:{text:$localize`:@@warehouse.form.code.label:Код`},isRequired:true,editorOptions:{placeholder:'WH-01',stylingMode:'outlined'}},
    {dataField:'name',label:{text:$localize`:@@warehouse.form.name.label:Название`},isRequired:true,editorOptions:{stylingMode:'outlined'}},
    {dataField:'address',label:{text:$localize`:@@warehouse.form.address.label:Адрес`},editorOptions:{stylingMode:'outlined'}},
    {dataField:'isActive',editorType:'dxCheckBox',label:{text:$localize`:@@warehouse.form.isActive.label:Активен`}},
  ];

  ngOnInit(){if(!this.isEdit)return;this.loading.set(true);this.svc.getWarehouseById(this.editId!).subscribe({next:wh=>{this.formData=wh;this.loading.set(false)},error:(e)=>{this.toast.showLoad(e);this.loading.set(false)}})}
  submit(){this.saving.set(true);this.err.set(null);
    const req:CreateWarehouseRequest={code:this.formData.code,name:this.formData.name,address:this.formData.address||null,isActive:this.formData.isActive??true};
    const d=()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})};const f=(e:any)=>{this.toast.showSave(e);this.saving.set(false)};
    if(this.isEdit){(this.svc.updateWarehouse(this.editId!,req)as any).subscribe({next:d,error:f})}else{(this.svc.createWarehouse(req)as any).subscribe({next:d,error:f})}
  }
  cancel(){void this.router.navigate(['..'],{relativeTo:this.route})}
}