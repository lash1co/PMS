import { Component, EventEmitter, Input, Output, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BillingService } from '../../../services/billing/billing.service';
import { InvoiceDetailedView } from '../../../Entities/Billing/billing-history';

@Component({
  selector: 'app-billing-history-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './billing-history-modal.component.html',
})
export class BillingHistoryModalComponent implements OnInit {
  @Input() invoiceId!: number;
  @Output() closeModal = new EventEmitter<void>();

  private billingService = inject(BillingService);
  
  invoiceDetail = signal<InvoiceDetailedView | null>(null);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.loadDetails();
  }

  private loadDetails() {
    this.isLoading.set(true);
    this.billingService.getInvoiceDetailedSummary(this.invoiceId).subscribe({
      next: (result) => {
        this.invoiceDetail.set(result);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching invoice details', err);
        this.isLoading.set(false);
      }
    });
  }

  onClose() {
    this.closeModal.emit();
  }
}