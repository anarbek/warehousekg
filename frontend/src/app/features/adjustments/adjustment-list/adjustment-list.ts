import { AfterViewInit, Component, OnInit, ViewChild, inject, signal } from "@angular/core";
import { DatePipe, LowerCasePipe } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatPaginator, MatPaginatorModule } from "@angular/material/paginator";
import { MatProgressBarModule } from "@angular/material/progress-bar";
import { MatSort, MatSortModule } from "@angular/material/sort";
import { MatTableDataSource, MatTableModule } from "@angular/material/table";
import { RouterLink } from "@angular/router";
import { StockAdjustmentSummary } from "../models/adjustment.model";
import { AdjustmentsService } from "../services/adjustments.service";

@Component({selector:"app-adjustment-list",imports:[DatePipe,LowerCasePipe,MatTableModule,MatPaginatorModule,MatSortModule,MatButtonModule,MatIconModule,MatProgressBarModule,RouterLink],templateUrl:"./adjustment-list.html",styleUrl:"./adjustment-list.scss"})
export class AdjustmentList implements OnInit, AfterViewInit {
  private readonly svc=inject(AdjustmentsService);
  protected readonly cols=["number","warehouseId","reason","status","lineCount","adjustedAtUtc","actions"];
  protected readonly ds=new MatTableDataSource<StockAdjustmentSummary>([]);
  protected readonly loading=signal(false);protected readonly error=signal<string|null>(null);
  @ViewChild(MatPaginator) paginator!:MatPaginator;@ViewChild(MatSort) sort!:MatSort;
  ngOnInit():void{this.load()}ngAfterViewInit():void{this.ds.paginator=this.paginator;this.ds.sort=this.sort}
  protected load():void{this.loading.set(true);this.error.set(null);this.svc.getAdjustments().subscribe({next:d=>{this.ds.data=d;this.loading.set(false)},error:()=>{this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);this.loading.set(false)}})}
}
