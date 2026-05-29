import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppointmentEfficiencyReportDto } from '../../../../models/analytics/reports.model';

@Component({
  selector: 'app-efficiency-report',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="a4-page">
      <div class="doc-header">
        <div class="logo-area">
          <h1>PMS</h1><p>Operational Efficiency Report</p>
        </div>
        <div class="meta-area">
          <p><strong>Period:</strong> {{ data.reportPeriod }}</p>
          <p><strong>Generated:</strong> {{ data.generatedAt | date:'dd/MM/yyyy HH:mm' }}</p>
        </div>
      </div>
      <hr class="divider">
      
      <div class="stats-grid">
        <div class="stat-box"><span class="label">Total Scheduled</span><span class="value">{{ data.totalScheduled }}</span></div>
        <div class="stat-box outline-green"><span class="label">Completed Successfully</span><span class="value text-green">{{ data.totalCompleted }}</span></div>
        <div class="stat-box" style="border-color: #f5c6cb; background: #fff3cd;"><span class="label">Cancelled / No-Shows</span><span class="value" style="color: #dc3545;">{{ data.totalCancelled }}</span></div>
      </div>

      <div class="comparison-chart">
        <h4 style="margin-top: 0; color: #34495e; text-transform: uppercase; font-size: 13px;">Monthly Efficiency Comparison</h4>
        
        <div class="bar-row">
          <span class="bar-label">Previous Month</span>
          <div class="bar-track">
            <div class="bar prev" [style.width.%]="data.previousMonthCompletionRate"></div>
          </div>
          <span class="bar-value">{{ data.previousMonthCompletionRate }}%</span>
        </div>
        
        <div class="bar-row">
          <span class="bar-label">Current Month</span>
          <div class="bar-track">
            <div class="bar current" [style.width.%]="data.completionRate"></div>
          </div>
          <span class="bar-value text-green">{{ data.completionRate }}%</span>
        </div>

        <p class="growth-indicator" [ngClass]="{'positive': data.growthPercentage >= 0, 'negative': data.growthPercentage < 0}">
          <strong style="color: #6c757d;">Variation vs previous month:</strong> 
          {{ data.growthPercentage > 0 ? '+' : '' }}{{ data.growthPercentage }}%
        </p>
      </div>

      <div class="table-section" style="margin-top: 30px;">
        <h3 style="font-size: 14px; border-bottom: 1px solid #dee2e6; padding-bottom: 8px;">Period Appointments</h3>
        <table class="report-table">
          <thead>
            <tr>
              <th>Date & Time</th>
              <th>Patient</th>
              <th>Assigned Doctor</th>
              <th>Reason</th>
              <th class="text-right">Status</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let appt of data.appointmentsReference" class="print-row">
              <td style="white-space: nowrap;">{{ appt.date | date:'dd/MM/yy HH:mm' }}</td>
              <td><strong>{{ appt.patientName }}</strong></td>
              <td>{{ appt.doctorName }}</td>
              <td><span style="font-size: 12px; color: #6c757d;">{{ appt.reason }}</span></td>
              <td class="text-right">
                <span [ngStyle]="{'color': appt.status === 'Completed' ? '#28a745' : (appt.status === 'Cancelled' ? '#dc3545' : '#ffc107')}">
                  <strong>{{ appt.status }}</strong>
                </span>
              </td>
            </tr>
            <tr *ngIf="data.appointmentsReference.length === 0">
              <td colspan="5" class="text-center">No appointments recorded for this period.</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="doc-footer"><p>Operational efficiency analysis and completeness metrics report.</p></div>
    </div>
  `,
  styles: [`
    .comparison-chart { margin: 25px 0; padding: 20px; background: #f8f9fa; border-radius: 8px; border: 1px solid #e9ecef; }
    .bar-row { display: flex; align-items: center; margin: 12px 0; }
    .bar-label { width: 110px; font-size: 13px; font-weight: 600; color: #495057; }
    
    .bar-track { flex: 1; height: 16px; background: #e9ecef; border-radius: 8px; margin: 0 15px; overflow: hidden; }
    .bar { height: 100%; border-radius: 8px; }
    .bar.prev { background: #adb5bd; }   
    .bar.current { background: #3f51b5; }    
    
    .bar-value { width: 50px; text-align: right; font-size: 13px; font-weight: bold; }
    .growth-indicator { text-align: right; margin-top: 15px; font-size: 13px; margin-bottom: 0; }
    
    .positive { color: #28a745; }
    .negative { color: #dc3545; }

    .print-row { page-break-inside: avoid; }
  `]
})
export class EfficiencyReportComponent {
  @Input({ required: true }) data!: AppointmentEfficiencyReportDto;
}