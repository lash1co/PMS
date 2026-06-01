import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MonthlyClosingReportDto } from '../../../../models/analytics/reports.model';

@Component({
  selector: 'app-closing-report',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="a4-page">
      <div class="doc-header">
        <div class="logo-area">
          <h1>PMS</h1><p>Monthly Closing Report</p>
        </div>
        <div class="meta-area">
          <p><strong>Period:</strong> {{ data.reportPeriod }}</p>
          <p><strong>Generated:</strong> {{ data.generatedAt | date:'dd/MM/yyyy HH:mm' }}</p>
        </div>
      </div>
      <hr class="divider">
      <div class="stats-grid">
        <div class="stat-box outline-green"><span class="label">Total Revenue</span><span class="value text-green">{{ data.summary.totalRevenue | currency }}</span></div>
        <div class="stat-box"><span class="label">Completed Appointments</span><span class="value">{{ data.summary.completedAppointments }} / {{ data.summary.totalAppointments }}</span></div>
        <div class="stat-box"><span class="label">New Patients</span><span class="value">{{ data.summary.newPatients }}</span></div>
      </div>
      <table class="report-table">
        <thead><tr><th>Doctor</th><th class="text-center">Appointments</th><th class="text-center">Encounters</th><th class="text-right">Revenue</th></tr></thead>
        <tbody>
          <tr *ngFor="let doc of data.doctorMetrics">
            <td><strong>{{ doc.doctorName }}</strong></td>
            <td class="text-center">{{ doc.appointmentsAttended }}</td>
            <td class="text-center">{{ doc.encountersCompleted }}</td>
            <td class="text-right">{{ doc.revenueGenerated | currency }}</td>
          </tr>
        </tbody>
      </table>
      <div class="doc-footer"><p>Document valid for internal audit purposes.</p></div>
    </div>
  `
})
export class ClosingReportComponent {
  @Input({ required: true }) data!: MonthlyClosingReportDto;
}