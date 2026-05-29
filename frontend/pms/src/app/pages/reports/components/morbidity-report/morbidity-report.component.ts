import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MorbidityReportDto } from '../../../../models/analytics/reports.model';

@Component({
  selector: 'app-morbidity-report',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="a4-page">
      <div class="doc-header">
        <div class="logo-area">
          <h1>PMS</h1><p>Morbidity Index</p>
        </div>
        <div class="meta-area">
          <p><strong>Period:</strong> {{ data.reportPeriod }}</p>
          <p><strong>Generated:</strong> {{ data.generatedAt | date:'dd/MM/yyyy HH:mm' }}</p>
        </div>
      </div>
      <hr class="divider">
      <div class="stats-grid">
        <div class="stat-box"><span class="label">Total Registered Diagnoses</span><span class="value">{{ data.totalDiagnoses }}</span></div>
      </div>
      <table class="report-table">
        <thead><tr><th>CIE-10 / Code</th><th>Diagnosis (Condition)</th><th class="text-center">Clinical Cases</th><th class="text-right">Proportion (%)</th></tr></thead>
        <tbody>
          <tr *ngFor="let cond of data.topConditions">
            <td><strong>{{ cond.conditionCode }}</strong></td>
            <td>{{ cond.conditionName }}</td>
            <td class="text-center">{{ cond.frequency }}</td>
            <td class="text-right">{{ cond.percentage }}%</td>
          </tr>
        </tbody>
      </table>
      <div class="doc-footer"><p>Document valid for internal audit purposes.</p></div>
    </div>
  `
})
export class MorbidityReportComponent {
  @Input({ required: true }) data!: MorbidityReportDto;
}