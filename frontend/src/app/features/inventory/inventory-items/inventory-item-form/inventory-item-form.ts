import { Component, OnInit, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { DxFormModule, DxButtonModule, DxProgressBarModule } from 'devextreme-angular';
import { ActivatedRoute, Router } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';

@Component({selector:'app-inventory-item-form',imports:[DxFormModule,DxButtonModule,DxProgressBarModule],templateUrl:'./inventory-item-form.html',styleUrl:'./inventory-item-form.scss'})
export class InventoryItemForm implements OnInit {
  private readonly svc=inject(InventoryService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);
  protected readonly editId=this.route.snapshot.paramMap.get('id');protected readonly isEdit=this.editId!==null;
  protected readonly saving=signal(false);protected readonly loading=signal(false);protected readonly err=signal<string|null>(null);
  protected readonly title=this.isEdit?'Edit item':'New item';
  protected formData:any={sku:'',name:'',description:'',barcode:'',categoryId:'',unitOfMeasureId:'',quantityOnHand:0,reorderLevel:0,isActive:true};
  protected readonly formItems=signal<any[]>([]);
  protected readonly ready=signal(false);

  ngOnInit(){
    this.loading.set(true);
    forkJoin({c:this.svc.getItemCategories(),u:this.svc.getUnitsOfMeasure()}).subscribe({
      next:d=>{
        const cats=d.c; const uoms=d.u;
        this.formItems.set([
          {dataField:'sku',label:{text:'SKU'},isRequired:true,editorOptions:{stylingMode:'outlined'}},
          {dataField:'name',label:{text:'Название'},isRequired:true,editorOptions:{stylingMode:'outlined'}},
          {dataField:'description',label:{text:'Описание'},editorOptions:{stylingMode:'outlined'}},
          {dataField:'barcode',label:{text:'Штрихкод'},editorOptions:{stylingMode:'outlined'}},
          {dataField:'categoryId',editorType:'dxSelectBox',label:{text:'Категория'},isRequired:true,editorOptions:{dataSource:cats,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
          {dataField:'unitOfMeasureId',editorType:'dxSelectBox',label:{text:'Ед. измерения'},isRequired:true,editorOptions:{dataSource:uoms,displayExpr:'name',valueExpr:'id',stylingMode:'outlined'}},
          {dataField:'quantityOnHand',label:{text:'На складе'},editorOptions:{stylingMode:'outlined'}},
          {dataField:'reorderLevel',label:{text:'Мин. остаток'},editorOptions:{stylingMode:'outlined'}},
          {dataField:'isActive',editorType:'dxCheckBox',label:{text:'Активен'}},
        ]);
        this.ready.set(true);this.loading.set(false);
        if(this.isEdit){this.svc.getInventoryItemById(this.editId!).subscribe({next:it=>{this.formData=it},error:()=>{this.err.set('Load error')}})}
      },
      error:()=>{this.err.set('Load error');this.loading.set(false)}
    });
  }
  submit(){this.saving.set(true);this.err.set(null);const r:any={...this.formData,categoryId:this.formData.categoryId,unitOfMeasureId:this.formData.unitOfMeasureId,quantityOnHand:Number(this.formData.quantityOnHand),reorderLevel:Number(this.formData.reorderLevel)};
    const d=()=>{this.saving.set(false);void this.router.navigate(['..'],{relativeTo:this.route})};const f=()=>{this.err.set('Save error');this.saving.set(false)};
    if(this.isEdit){(this.svc.updateInventoryItem(this.editId!,r)as any).subscribe({next:d,error:f})}else{(this.svc.createInventoryItem(r)as any).subscribe({next:d,error:f})}}
  cancel(){void this.router.navigate(['..'],{relativeTo:this.route})}
}