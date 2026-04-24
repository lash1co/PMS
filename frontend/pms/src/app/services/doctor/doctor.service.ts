import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DoctorService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/doctors`;

  searchDoctors(term: string) {
    return this.http.get<any[]>(`${this.apiUrl}/search?searchTerm=${term}`
        //, {headers: { Authorization: `Bearer ${localStorage.getItem('pms_token')}` }}
    );
  }
}