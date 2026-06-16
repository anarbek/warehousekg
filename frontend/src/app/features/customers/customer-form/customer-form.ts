import { Component, OnInit, inject, signal } from "@angular/core";
import { Observable } from "rxjs";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { ActivatedRoute, Router } from "@angular/router";
import { CreateCustomerRequest } from "../models/customer-so.model";
import { CustomerSoService } from "../services/customer-so.service";

@Component({selector:"app-customer-form",imports:[ReactiveFormsModule,MatCardModule,MatFormFieldModule,MatInputModule,MatCheckboxModule,MatButtonModule,MatIconModule,MatProgressBarModule],templateUrl:"./customer-form.html",styleUrl:"./customer-form.scss"})
export class CustomerForm implements OnInit {
  private readonly fb=inject(FormBuilder);private readonly svc=inject(CustomerSoService);private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);
  protected readonly editId=this.route.snapshot.paramMap.get("id");protected readonly isEditMode=this.editId!==null;protected readonly saving=signal(false);protected readonly loading=signal(false);protected readonly error=signal<string|null>(null);
  protected readonly title=this.isEditMode?$localize`:@@cust.form.edit:????????????? ???????`:$localize`:@@cust.form.create:????? ??????`;
  protected readonly form=this.fb.group({code:["",Validators.required],name:["",Validators.required],contactName:[""],email:[""],phone:[""],address:[""],taxId:[""],isActive:[true]});
  ngOnInit():void{if(!this.isEditMode)return;this.loading.set(true);this.svc.getCustomerById(this.editId!).subscribe({next:s=>{this.form.patchValue(s);this.loading.set(false)},error:()=>{this.error.set($localize`:@@common.loadError:?? ??????? ????????? ??????`);this.loading.set(false)}})}
  protected submit():void{if(this.form.invalid)return;this.saving.set(true);this.error.set(null);const v=this.form.getRawValue();const r:CreateCustomerRequest={code:v.code!,name:v.name!,contactName:v.contactName||null,email:v.email||null,phone:v.phone||null,address:v.address||null,taxId:v.taxId||null,isActive:v.isActive ?? true};const op:Observable<unknown>=this.isEditMode?this.svc.updateCustomer(this.editId!,r):this.svc.createCustomer(r);op.subscribe({next:()=>{this.saving.set(false);void this.router.navigate([".."],{relativeTo:this.route})},error:()=>{this.error.set($localize`:@@common.saveError:?? ??????? ????????? ??????`);this.saving.set(false)}})}
  protected cancel():void{void this.router.navigate([".."],{relativeTo:this.route})}
}
