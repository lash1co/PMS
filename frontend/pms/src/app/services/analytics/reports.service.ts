import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  MonthlyClosingReportDto, 
  MorbidityReportDto, 
  AccountsReceivableReportDto, 
  AppointmentEfficiencyReportDto 
} from '../../models/analytics/reports.model';

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

  getMorbidityReport(year: number, month: number): Observable<MorbidityReportDto> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());

    return this.http.get<MorbidityReportDto>(`${this.apiUrl}/morbidity`, { params });
  }

  getAccountsReceivableReport(year: number, month: number): Observable<AccountsReceivableReportDto> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());

    return this.http.get<AccountsReceivableReportDto>(`${this.apiUrl}/accounts-receivable`, { params });
  }

  getAppointmentEfficiencyReport(year: number, month: number): Observable<AppointmentEfficiencyReportDto> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());

    return this.http.get<AppointmentEfficiencyReportDto>(`${this.apiUrl}/appointment-efficiency`, { params });
  }
}