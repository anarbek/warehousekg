import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, LowerCasePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, Observable } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { PurchaseOrder, PurchaseOrderStatus } from '../../models/supplier-po.model';
import { InventoryItem } from '../../../inventory/models/inventory-item.model';
import { InventoryService } from '../../../inventory/services/inventory.service';
import { SupplierPoService } from '../../services/supplier-po.service';

@Component({
  selector: 'app-po-detail',
  imports: [FormsModule, DatePipe, LowerCasePipe, DecimalPipe, MatCardModule, MatButtonModule, MatIconModule, MatProgressBarModule, MatTableModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  templateUrl: './po-detail.html',
  styleUrl: './po-detail.scss',
})
export class PoDetail implements OnInit {
  private readonly service = inject(SupplierPoService);
  private readonly inventoryService = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly po = signal<PurchaseOrder | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly items = signal<InventoryItem[]>([]);
  protected readonly saving = signal(false);
  protected readonly lineColumns = ['inventoryItemId', 'quantity', 'unitPrice', 'lineTotal', 'actions'];

  ngOnInit(): void { this.load(); }

  protected load(): void {
    this.loading.set(true); this.error.set(null);
    forkJoin({ po: this.service.getPurchaseOrderById(this.id), items: this.inventoryService.getInventoryItems() })
    .subscribe({ next: ({po,items}) => { this.po.set(po); this.items.set(items); this.loading.set(false); },
      error: () => { this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`); this.loading.set(false); },
    });
  }

  protected canTransition(target: PurchaseOrderStatus): boolean {
    const s = this.po()?.status;
    if (!s) return false;
    if (target === 'Submitted') return s === 'Draft';
    if (target === 'Received') return s === 'Submitted';
    if (target === 'Cancelled') return s === 'Draft' || s === 'Submitted';
    return false;
  }

  protected transition(action: 'submit' | 'receive' | 'cancel'): void {
    this.saving.set(true);
    const op: Observable<void> = action === 'submit' ? this.service.submitPO(this.id) : action === 'receive' ? this.service.receivePO(this.id) : this.service.cancelPO(this.id);
    op.subscribe({ next: () => { this.saving.set(false); this.load(); }, error: () => { this.saving.set(false); this.load(); } });
  }

  protected goBack(): void { void this.router.navigate(['..'], { relativeTo: this.route }); }

  protected getItemName(itemId: string): string {
    const it = this.items().find(i => i.id === itemId);
    return it ? `${it.sku} — ${it.name}` : itemId;
  }

  protected removeLine(index: number): void { this.po.update(p => { if(p){ p.lines.splice(index,1); p.totalAmount = p.lines.reduce((s,l)=>s+l.lineTotal,0); } return p; }); }
  protected addLine(): void { this.po.update(p => { if(p){ p.lines.push({ inventoryItemId:'', inventoryItemName:'', quantity:0, unitPrice:0, lineTotal:0 }); } return p; }); }
  protected updateLineTotal(line: any): void { line.lineTotal = (line.quantity || 0) * (line.unitPrice || 0); }
}
