import { Component, OnInit, inject, signal } from '@angular/core';
import { DxFormModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';

@Component({selector:'app-item-category-form',imports:[DxFormModule,DxButtonModule,DxProgressBarModule],templateUrl:'./item-category-form.html',styleUrl:'./item-category-form.scss'})
export class ItemCategoryForm implements OnInit {
  private readonly svc=inject(InventoryService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);
  protected readonly editId=this.route.snapshot.paramMap.get('id');protected readonly isEdit=this.editId!==null;
  protected readonly saving=signal(false);protected readonly loading=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly title=this.isEdit?'Edit category':'New category';
  protected formData:any={code:'',name:'',isActive:true};
  protected readonly formItems:any[]=[
    {dataField:'code',label:{text:'Код'},isRequired:true,editorOptions:{stylingMode:'outlined'}},
    {dataField:'name',label:{text:'Название'},isRequired:true,editorOptions:{stylingMode:'outlined'}},
    {dataField:'isActive',editorType:'dxCheckBox',label:{text:'Активна'}},
  ];
  ngOnInit(){if(!this.isEdit)return;this.loading.set(true);this.svc.getItemCategoryById(this.editId!).subscribe({next:c=>{this.formData=c;this.loading.set(false)},error:()=>{this.err.set('Load error');this.loading.set(false)}})}
  submit(){this.saving.set(true);this.err.set(null);
    const d=()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})};const f=()=>{this.err.set('Save error');this.saving.set(false)};
    (this.isEdit?(this.svc.updateItemCategory(this.editId!,this.formData)as any):(this.svc.createItemCategory(this.formData)as any)).subscribe({next:d,error:f})}
  cancel(){void this.router.navigate(['..'],{relativeTo:this.route})}
}