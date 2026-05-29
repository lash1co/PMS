import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ReportsService } from '../../services/analytics/reports.service';
import { MonthlyClosingReportDto } from '../../models/analytics/reports.model';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent {
  private reportsService = inject(ReportsService);

  selectedYear = signal<number>(new Date().getFullYear());
  selectedMonth = signal<number>(new Date().getMonth() + 1);
  
  reportData = signal<MonthlyClosingReportDto | null>(null);
  isLoading = signal<boolean>(false);

  years: number[] = [2024, 2025, 2026, 2027];
  months = [
    { value: 1, name: 'January' }, { value: 2, name: 'February' },
    { value: 3, name: 'March' }, { value: 4, name: 'April' },
    { value: 5, name: 'May' }, { value: 6, name: 'June' },
    { value: 7, name: 'July' }, { value: 8, name: 'August' },
    { value: 9, name: 'September' }, { value: 10, name: 'October' },
    { value: 11, name: 'November' }, { value: 12, name: 'December' }
  ];

  generateReport() {
    this.isLoading.set(true);

    this.reportsService.getMonthlyClosingReport(this.selectedYear(), this.selectedMonth())
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (data) => {
          this.reportData.set(data);
        },
        error: (err) => {
          console.error('Error al obtener el reporte', err);
        }
      });
  }

  printReport() {
    window.print();
  }
}