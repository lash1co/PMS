import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Patient } from '../../models/patient.model';

@Injectable({ providedIn: 'root' })
export class PatientService {
  private apiUrl = 'http://localhost:5231/api/patients';
  private registrationUrl = 'http://localhost:5231/api/patientregistration';

  constructor(private http: HttpClient) { }

  // Get all patients
  getPatients(): Observable<Patient[]> {
    return this.http.get<Patient[]>(this.apiUrl);
  }

  // Get patient by id
  getPatientById(id: number): Observable<Patient> {
    return this.http.get<Patient>(`${this.apiUrl}/${id}`);
  }

  // Get patient by name
  searchPatients(term: string): Observable<Patient[]> {
    return this.http.get<Patient[]>(`${this.apiUrl}/search?searchTerm=${term}`);
  }

  // Create patient
  createPatient(patient: Patient): Observable<Patient> {
    return this.http.post<Patient>(this.apiUrl, patient);
  }

  // Update patient by id
  updatePatient(id: number, patient: Patient): Observable<Patient> {
    return this.http.put<Patient>(`${this.apiUrl}/${id}`, patient);
  }

  // Delete patient by id
  deletePatient(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  generateInviteLink(id: number): Observable<{url: string}> {
    return this.http.post<{url: string}>(`${this.registrationUrl}/${id}/generate-invite`, {});
  }

  getInviteDetails(token: string): Observable<any> {
    const headers = new HttpHeaders().set('X-Invite-Token', token);
    return this.http.get(`${this.registrationUrl}/invite-details`, { headers });
  }

  completeRegistration(token: string, payload: any): Observable<any> {
    const headers = new HttpHeaders().set('X-Invite-Token', token);
    return this.http.put(`${this.registrationUrl}/complete`, payload, { headers });
  }
}