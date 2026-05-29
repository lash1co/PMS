import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ReportsService } from '../../services/analytics/reports.service';
import { 
  MonthlyClosingReportDto, 
  MorbidityReportDto, 
  AccountsReceivableReportDto, 
  AppointmentEfficiencyReportDto 
} from '../../models/analytics/reports.model';
import { EfficiencyReportComponent } from './components/efficiency-report/efficiency-report.component';
import { ReceivableReportComponent } from './components/receivable-report/receivable-report.component';
import { MorbidityReportComponent } from './components/morbidity-report/morbidity-report.component';
import { ClosingReportComponent } from './components/closing-report/closing-report.component';

type ReportType = 'closing' | 'morbidity' | 'receivable' | 'efficiency';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, ClosingReportComponent, MorbidityReportComponent, ReceivableReportComponent, EfficiencyReportComponent],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent {
  private reportsService = inject(ReportsService);

  selectedReportType = signal<ReportType>('closing');
  selectedYear = signal<number>(new Date().getFullYear());
  selectedMonth = signal<number>(new Date().getMonth() + 1);
  isLoading = signal<boolean>(false);

  closingData = signal<MonthlyClosingReportDto | null>(null);
  morbidityData = signal<MorbidityReportDto | null>(null);
  receivableData = signal<AccountsReceivableReportDto | null>(null);
  efficiencyData = signal<AppointmentEfficiencyReportDto | null>(null);

  years: number[] = [2024, 2025, 2026, 2027];
  months = [
    { value: 1, name: 'January' }, { value: 2, name: 'February' }, { value: 3, name: 'March' }, { value: 4, name: 'April' },
    { value: 5, name: 'May' }, { value: 6, name: 'June' }, { value: 7, name: 'July' }, { value: 8, name: 'August' },
    { value: 9, name: 'September' }, { value: 10, name: 'October' }, { value: 11, name: 'November' }, { value: 12, name: 'December' }
  ];

  generateReport() {
    this.isLoading.set(true);
    
    this.closingData.set(null);
    this.morbidityData.set(null);
    this.receivableData.set(null);
    this.efficiencyData.set(null);

    const type = this.selectedReportType();
    const y = this.selectedYear();
    const m = this.selectedMonth();

    if (type === 'closing') {
      this.reportsService.getMonthlyClosingReport(y, m).pipe(finalize(() => this.isLoading.set(false)))
        .subscribe(data => this.closingData.set(data));
    } 
    else if (type === 'morbidity') {
      this.reportsService.getMorbidityReport(y, m).pipe(finalize(() => this.isLoading.set(false)))
        .subscribe(data => this.morbidityData.set(data));
    } 
    else if (type === 'receivable') {
      this.reportsService.getAccountsReceivableReport(y, m).pipe(finalize(() => this.isLoading.set(false)))
        .subscribe(data => this.receivableData.set(data));
    } 
    else if (type === 'efficiency') {
      this.reportsService.getAppointmentEfficiencyReport(y, m).pipe(finalize(() => this.isLoading.set(false)))
        .subscribe(data => this.efficiencyData.set(data));
    }
  }

  printReport() {
    window.print();
  }

  hasData(): boolean {
    return !!(this.closingData() || this.morbidityData() || this.receivableData() || this.efficiencyData());
  }
}