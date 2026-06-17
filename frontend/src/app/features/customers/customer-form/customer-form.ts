import { Component, OnInit, inject, signal } from '@angular/core';
import { DxFormModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomerSoService } from '../services/customer-so.service';

@Component({selector:'app-customer-form',imports:[DxFormModule,DxButtonModule,DxProgressBarModule],templateUrl:'./customer-form.html',styleUrl:'./customer-form.scss'})
export class CustomerForm implements OnInit {
  private readonly svc=inject(CustomerSoService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);
  protected readonly editId=this.route.snapshot.paramMap.get('id');protected readonly isEdit=this.editId!==null;
  protected readonly saving=signal(false);protected readonly loading=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly title=this.isEdit?$localize`:@@cust.form.edit:Редактировать клиента`:$localize`:@@cust.form.create:Новый клиент`;
  protected formData:any={code:'',name:'',contactName:'',email:'',phone:'',address:'',taxId:'',isActive:true};
  protected readonly formItems:any[]=[
    {dataField:'code',label:{text:$localize`:@@cust.form.code:Код`},isRequired:true,editorOptions:{stylingMode:'outlined'}},
    {dataField:'name',label:{text:$localize`:@@cust.form.name:Название`},isRequired:true,editorOptions:{stylingMode:'outlined'}},
    {dataField:'contactName',label:{text:$localize`:@@cust.form.contact:Контактное лицо`},editorOptions:{stylingMode:'outlined'}},
    {dataField:'email',label:{text:'Email'},editorOptions:{stylingMode:'outlined'}},
    {dataField:'phone',label:{text:$localize`:@@cust.form.phone:Телефон`},editorOptions:{stylingMode:'outlined'}},
    {dataField:'address',label:{text:$localize`:@@cust.form.address:Адрес`},editorOptions:{stylingMode:'outlined'}},
    {dataField:'taxId',label:{text:'Tax ID'},editorOptions:{stylingMode:'outlined'}},
    {dataField:'isActive',editorType:'dxCheckBox',label:{text:$localize`:@@cust.form.active:Активен`}},
  ];
  ngOnInit(){if(!this.isEdit)return;this.loading.set(true);this.svc.getCustomerById(this.editId!).subscribe({next:s=>{this.formData=s;this.loading.set(false)},error:()=>{this.err.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
  submit(){this.saving.set(true);this.err.set(null);const r:any={...this.formData,contactName:this.formData.contactName||null,email:this.formData.email||null,phone:this.formData.phone||null,address:this.formData.address||null,taxId:this.formData.taxId||null};
    const d=()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})};const f=()=>{this.err.set($localize`:@@common.saveError:Не удалось сохранить данные`);this.saving.set(false)};
    if(this.isEdit){(this.svc.updateCustomer(this.editId!,r)as any).subscribe({next:d,error:f})}else{(this.svc.createCustomer(r)as any).subscribe({next:d,error:f})}}
  cancel(){void this.router.navigate(['..'],{relativeTo:this.route})}
}