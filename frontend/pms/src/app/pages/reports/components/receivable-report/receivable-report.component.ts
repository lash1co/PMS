import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountsReceivableReportDto } from '../../../../models/analytics/reports.model';

@Component({
  selector: 'app-receivable-report',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="a4-page">
      <div class="doc-header">
        <div class="logo-area">
          <h1>PMS</h1><p>Accounts Receivable Statement</p>
        </div>
        <div class="meta-area">
          <p><strong>Historical Cut-off:</strong> {{ data.generatedAt | date:'dd/MM/yyyy HH:mm' }}</p>
        </div>
      </div>
      <hr class="divider">
      <div class="stats-grid">
        <div class="stat-box"><span class="label">Pending Debt (On Time)</span><span class="value text-green">{{ data.totalPendingAmount | currency }}</span></div>
        <div class="stat-box" style="border-color: #f5c6cb; background: #fff3cd;"><span class="label">Overdue Debt (Alert)</span><span class="value" style="color: #dc3545;">{{ data.totalOverdueAmount | currency }}</span></div>
      </div>
      <table class="report-table">
        <thead><tr><th>Patient</th><th>Phone</th><th class="text-center">Invoice #</th><th>Due Date</th><th class="text-right">Amount Owed</th></tr></thead>
        <tbody>
          <tr *ngFor="let debtor of data.debtors" [style.color]="debtor.isOverdue ? '#dc3545' : 'inherit'">
            <td><strong>{{ debtor.patientName }}</strong> <span *ngIf="debtor.isOverdue">⚠️</span></td>
            <td>{{ debtor.phone }}</td>
            <td class="text-center">#{{ debtor.invoiceId }}</td>
            <td>{{ debtor.dueDate | date:'dd/MM/yyyy' }}</td>
            <td class="text-right"><strong>{{ debtor.amountOwed | currency }}</strong></td>
          </tr>
          <tr *ngIf="data.debtors.length === 0">
            <td colspan="5" class="text-center">No pending accounts receivable.</td>
          </tr>
        </tbody>
      </table>
      <div class="doc-footer"><p>Contact patients with alerts (⚠️) at the earliest convenience.</p></div>
    </div>
  `
})
export class ReceivableReportComponent {
  @Input({ required: true }) data!: AccountsReceivableReportDto;
}