import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { InvoiceHistoryFilter, InvoiceHistoryResult, InvoiceDetailedView } from '../../Entities/Billing/billing-history';

@Injectable({
  providedIn: 'root'
})

export class BillingService {
  private apiUrl = `${environment.apiUrl}/api/invoices`;
  constructor(private http: HttpClient) { }

  getPendingInvoices(): Observable<PendingInvoiceInterface[]> {
    return this.http.get<PendingInvoiceInterface[]>(this.apiUrl, {
      headers: this.getAuthHeaders()
    });
  }

  saveInvoiceBilling(billing: PendingInvoiceInterface): Observable<any> {
    var billingUri = `${this.apiUrl}/createBilling`;
    return this.http.post<any>(billingUri, billing, {
      headers: this.getAuthHeaders()
    });
  }

  getActiveInvoices(): Observable<ActiveInvoiceInterface[]> {
    return this.http.get<ActiveInvoiceInterface[]>(`${this.apiUrl}/active`, {
      headers: this.getAuthHeaders()
    });
  }

  registerInvoicePayment(invoiceId: number, payment: RegisterPaymentInterface): Observable<ActiveInvoiceInterface> {
    return this.http.post<ActiveInvoiceInterface>(`${this.apiUrl}/${invoiceId}/payments`, payment, {
      headers: this.getAuthHeaders()
    });
  }

  private getAuthHeaders(): { Authorization: string } {
    const token = typeof localStorage === 'undefined' ? '' : localStorage.getItem('pms_token');
    return { Authorization: `Bearer ${token}` };
  }

  /**
   * Retrieves the filtered history of invoices.
   */
  getInvoiceHistory(filter: InvoiceHistoryFilter): Observable<InvoiceHistoryResult[]> {
    return this.http.post<InvoiceHistoryResult[]>(`${this.apiUrl}/history`, filter);
  }

  /**
   * Retrieves the comprehensive detail view of a specific invoice.
   */
  getInvoiceDetailedSummary(invoiceId: number): Observable<InvoiceDetailedView> {
    return this.http.get<InvoiceDetailedView>(`${this.apiUrl}/${invoiceId}/detailed-summary`);
  }

}
