import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-billing-review',
  imports: [CommonModule, FormsModule],
  templateUrl: './billing-review.component.html',
  styleUrl: './billing-review.component.css',
})
export class BillingReviewComponent implements OnInit {
  @Input() billing: PendingInvoiceInterface = {
    encounterId: 0,
    patientName: '',
    encounterDate: '',
    invoiceDetails: []
  };

  @Output() save = new EventEmitter<PendingInvoiceInterface>();
  @Output() close = new EventEmitter<void>();
  isSaving = signal<boolean>(false);

  ngOnInit(){
    let detCounter = 1;
    this.billing.invoiceDetails.forEach(detail => {
      detail.detailId = detCounter;
      detCounter++;
    });
  }

  onIUnitPriceChange(detail: PendingInvoiceDetailInterface, newUnitPrice: number): void {
    detail.unitPrice = Number(newUnitPrice) || 0;
    detail.total = detail.quantity * detail.unitPrice;
  }

  onSave(): void {
    console.log('Saving billing:', JSON.stringify(this.billing));
    this.isSaving.set(true);

    this.save.emit(this.billing);
    this.isSaving.set(false);
  }

  onClose(): void {
    this.close.emit();
  }
}
