import { Component, OnInit, Signal, signal } from '@angular/core';
import { BillingService } from '../../services/billing/billing.service';
import { BillingReviewComponent } from '../billing-review.component/billing-review.component';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Inject, PLATFORM_ID } from '@angular/core';

@Component({
  selector: 'app-billing.component',
  imports: [BillingReviewComponent, CommonModule, FormsModule],
  templateUrl: './billing.component.html',
  styleUrl: './billing.component.css',
})
export class BillingComponent implements OnInit {
  pendingBillingData = signal<PendingInvoiceInterface[]>([]);
  activeInvoices = signal<ActiveInvoiceInterface[]>([]);
  selectedInvoice = signal<ActiveInvoiceInterface | null>(null);
  activeTab = signal<'pending' | 'active'>('pending');
  paymentMethods = [
    'Cash',
    'Credit Card',
    'Debit Card',
    'Bank Transfer',
    'Wire Transfer',
    'Check',
    'ACH Payment',
    'Mobile Payment',
    'Digital Wallet'
  ];
  paymentForm: RegisterPaymentInterface = {
    amount: 0,
    paymentMethod: '',
    referenceNumber: '',
    notes: ''
  };
  currentPendingBilling: PendingInvoiceInterface = {
    encounterId: 0,
    patientName: '',
    encounterDate: '',
    invoiceDetails: []
  };
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  constructor(
    private billingService: BillingService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit() {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.getPendingInvoices();
    this.getActiveInvoices();
  }

  getPendingInvoices() {
    this.billingService.getPendingInvoices().subscribe((invoices) => {
      this.pendingBillingData.set(invoices);
      console.log(invoices);
    });
  }

  getActiveInvoices() {
    this.billingService.getActiveInvoices().subscribe((invoices) => {
      this.activeInvoices.set(invoices);
    });
  }

  selectTab(tab: 'pending' | 'active'): void {
    this.activeTab.set(tab);
    if (tab === 'active') {
      this.getActiveInvoices();
    } else {
      this.getPendingInvoices();
    }
  }

  openEditBilling(billing: PendingInvoiceInterface): void {
    this.isEditing.set(true);
    this.currentPendingBilling = { ...billing };
  }

  savePendingBilling(updatedBilling: PendingInvoiceInterface): void {
    this.isSaving.set(true);
    this.billingService.saveInvoiceBilling(updatedBilling).subscribe({
      next: () => {
        this.getPendingInvoices();
        this.getActiveInvoices();
        this.closeModal();
      },
      error: (error) => {
        console.error('Error saving billing:', error);
        alert('Failed to save billing. Please try again.');
        this.isSaving.set(false);
      }
    });
    console.log('Saving billing:', updatedBilling);
    setTimeout(() => {
      this.isSaving.set(false);
      this.closeModal();
    }, 1000);
  }

  closeModal(): void {
    this.isEditing.set(false);
    this.currentPendingBilling = {
      encounterId: 0,
      patientName: '',
      encounterDate: '',
      invoiceDetails: []
    };
  }

  openPayment(invoice: ActiveInvoiceInterface): void {
    this.selectedInvoice.set(invoice);
    this.paymentForm = {
      amount: invoice.balance,
      paymentMethod: '',
      referenceNumber: '',
      notes: ''
    };
  }

  closePayment(): void {
    this.selectedInvoice.set(null);
    this.paymentForm = {
      amount: 0,
      paymentMethod: '',
      referenceNumber: '',
      notes: ''
    };
  }

  savePayment(): void {
    const invoice = this.selectedInvoice();
    if (!invoice) {
      return;
    }

    if (this.paymentForm.amount <= 0 || this.paymentForm.amount > invoice.balance) {
      alert('Payment amount must be greater than zero and cannot exceed the invoice balance.');
      return;
    }

    if (!this.paymentForm.paymentMethod) {
      alert('Payment method is required.');
      return;
    }

    this.isSaving.set(true);
    this.billingService.registerInvoicePayment(invoice.id, this.paymentForm).subscribe({
      next: () => {
        this.getActiveInvoices();
        this.closePayment();
        this.isSaving.set(false);
      },
      error: (error) => {
        console.error('Error registering payment:', error);
        alert(error?.error?.error ?? 'Failed to register payment. Please try again.');
        this.isSaving.set(false);
      }
    });
  }
}
