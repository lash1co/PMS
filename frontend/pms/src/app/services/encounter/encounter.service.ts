import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class EncounterService {
  private apiUrl = 'http://localhost:5231/api/encounter';
  constructor(private http: HttpClient) { }

  getEncounters(): Observable<EncounterInterface[]> {
    return this.http.get<EncounterInterface[]>(this.apiUrl, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('pms_token')}`
      }
    });
  }
}
