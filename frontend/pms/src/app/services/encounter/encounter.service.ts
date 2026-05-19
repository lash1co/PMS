import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EncounterInterface, EncounterSummaryDto } from '../../Entities/Encounters/Encounter';
import { getPmsToken } from '../../utils/storage.util';

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

  getEncounterSummary(encounterId: number): Observable<EncounterSummaryDto> {
    return this.http.get<EncounterSummaryDto>(`${this.apiUrl}/${encounterId}/summary`, this.getAuthHeaders());
  }

  updateClinicalNote(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/note`, data, this.getAuthHeaders());
  }

  addObservation(id: number, data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/observations`, data, this.getAuthHeaders());
  }

  addAllergy(id: number, data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/allergies`, data, this.getAuthHeaders());
  }

  completeEncounter(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/complete`, {}, this.getAuthHeaders());
  }

  deleteObservation(encounterId: number, observationId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${encounterId}/observations/${observationId}`, this.getAuthHeaders());
  }

  deleteCondition(encounterId: number, conditionId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${encounterId}/conditions/${conditionId}`, this.getAuthHeaders());
  }

  deleteAllergy(encounterId: number, allergyId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${encounterId}/allergies/${allergyId}`, this.getAuthHeaders());
  }
}