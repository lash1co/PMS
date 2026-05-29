import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MonthlyClosingReportDto } from '../../models/analytics/reports.model';

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private apiUrl = `${environment.apiUrl}/api/Reports`; 

  constructor(private http: HttpClient) { }

  getMonthlyClosingReport(year: number, month: number): Observable<MonthlyClosingReportDto> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());
      
    return this.http.get<MonthlyClosingReportDto>(`${this.apiUrl}/monthly-closing`, { params });
  }
}