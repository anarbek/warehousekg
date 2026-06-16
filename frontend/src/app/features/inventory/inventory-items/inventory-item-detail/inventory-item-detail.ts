import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InventoryItem } from '../../models/inventory-item.model';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-inventory-item-detail',
  imports: [MatCardModule, MatButtonModule, MatIconModule, MatProgressBarModule, RouterLink],
  templateUrl: './inventory-item-detail.html',
  styleUrl: './inventory-item-detail.scss',
})
export class InventoryItemDetail implements OnInit {
  private readonly service = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly id = this.route.snapshot.paramMap.get('id')!;
  protected readonly item = signal<InventoryItem | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loading.set(true);
    this.service.getInventoryItemById(this.id).subscribe({
      next: (it) => {
        this.item.set(it);
        this.loading.set(false);
      },
      error: () => {
        this.error.set($localize`:@@common.loadError:Не удалось загрузить данные`);
        this.loading.set(false);
      },
    });
  }

  protected delete(): void {
    if (!confirm($localize`:@@common.confirmDelete:Вы уверены, что хотите удалить запись?`)) return;
    this.service.deleteInventoryItem(this.id).subscribe({
      next: () => void this.router.navigate(['../..'], { relativeTo: this.route }),
    });
  }
}
