// src/app/services/encounter/encounter.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EncounterHistoryResponse, EncounterInterface, EncounterSummaryDto, EncounterHistoryFilter, EncounterHistoryDetail } from '../../Entities/Encounters/Encounter';
import { getPmsToken } from '../../utils/storage.util';
import { HttpParams } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class EncounterService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/encounter`;

  private getAuthHeaders() {
    const token = getPmsToken();
    return { headers: { Authorization: `Bearer ${token}` } };
  }

  getEncounters(): Observable<EncounterInterface[]> {
    return this.http.get<EncounterInterface[]>(this.apiUrl, this.getAuthHeaders());
  }

  startEncounter(appointmentId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/start/${appointmentId}`, {}, this.getAuthHeaders());
  }

  startWalkIn(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/walk-in`, data, this.getAuthHeaders());
  }

  invalidateEncounter(id: number, reason: string): Observable<any> {
  return this.http.post(`${this.apiUrl}/${id}/invalidate`, { reason }, this.getAuthHeaders());
}

  getEncounterSummary(encounterId: number): Observable<EncounterSummaryDto> {
    return this.http.get<EncounterSummaryDto>(`${this.apiUrl}/${encounterId}/summary`, this.getAuthHeaders());
  }

  updateClinicalNote(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/note`, data, this.getAuthHeaders());
  }

  completeEncounter(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/complete`, {}, this.getAuthHeaders());
  }

  addObservation(id: number, data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/observations`, data, this.getAuthHeaders());
  }
  deleteObservation(encounterId: number, observationId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${encounterId}/observations/${observationId}`, this.getAuthHeaders());
  }

  addAllergy(id: number, data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/allergies`, data, this.getAuthHeaders());
  }
  deleteAllergy(encounterId: number, allergyId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${encounterId}/allergies/${allergyId}`, this.getAuthHeaders());
  }

  addCondition(id: number, data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/conditions`, data, this.getAuthHeaders());
  }
  deleteCondition(encounterId: number, conditionId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${encounterId}/conditions/${conditionId}`, this.getAuthHeaders());
  }

  addProcedure(id: number, data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/procedures`, data, this.getAuthHeaders());
  }
  deleteProcedure(encounterId: number, procedureId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${encounterId}/procedures/${procedureId}`, this.getAuthHeaders());
  }

  /**
   * Fetches the read-only encounter history using query parameters.
   * Arrays are mapped safely to HttpParams to ensure proper backend binding.
   */
  getEncounterHistory(filters: EncounterHistoryFilter): Observable<EncounterHistoryResponse[]> {
    let params = new HttpParams()
      .set('StartDate', filters.startDate)
      .set('EndDate', filters.endDate);

    if (filters.encounterType) {
      params = params.set('EncounterType', filters.encounterType);
    }

    if (filters.patientIds && filters.patientIds.length > 0) {
      filters.patientIds.forEach(id => { params = params.append('PatientIds', id); });
    }

    if (filters.doctorIds && filters.doctorIds.length > 0) {
      filters.doctorIds.forEach(id => { params = params.append('DoctorIds', id); });
    }

    return this.http.get<EncounterHistoryResponse[]>(`${this.apiUrl}/history`, {
      headers: this.getAuthHeaders().headers,
      params: params
    });
  }

  /**
   * Fetches comprehensive read-only details for a specific historical encounter.
   */
  getEncounterHistoryDetail(id: number): Observable<EncounterHistoryDetail> {
    return this.http.get<EncounterHistoryDetail>(`${this.apiUrl}/${id}/history-detail`, this.getAuthHeaders());
  }
}