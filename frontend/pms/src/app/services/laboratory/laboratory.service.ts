import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { getPmsToken } from '../../utils/storage.util';

@Injectable({
  providedIn: 'root'
})

export class LaboratoryService {
  private apiUrl = 'http://localhost:5231/api/Laboratory';

  constructor(private http: HttpClient) { }

  getLaboratories(): Observable<LaboratoryInterface[]> {
    return this.http.get<LaboratoryInterface[]>(this.apiUrl, {
      headers: {
        Authorization: `Bearer ${getPmsToken()}`
      }
    });
  }

  createLaboratory(laboratory: LaboratoryInterface): Observable<any> {
    return this.http.post<any>(this.apiUrl, laboratory, {
      headers: {
        Authorization: `Bearer ${getPmsToken()}`
      }
    });
  }

  updateLaboratory(laboratory: LaboratoryInterface): Observable<any> {
    return this.http.put<any>(this.apiUrl, laboratory, {
      headers: {
        Authorization: `Bearer ${getPmsToken()}`
      }
    });
  }

  deleteLaboratory(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: {
        Authorization: `Bearer ${getPmsToken()}`
      }
    });
  }
}
