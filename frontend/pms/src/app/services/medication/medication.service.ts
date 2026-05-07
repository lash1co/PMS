import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Medication } from '../../models/medication.model';

@Injectable({ providedIn: 'root' })
export class MedicationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/medications`;

  searchMedications(term: string) {
  return this.http.get<Medication[]>(
    `${this.apiUrl}/search?searchTerm=${term ?? ''}`
  );
}
}