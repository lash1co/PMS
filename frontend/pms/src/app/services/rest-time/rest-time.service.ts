import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class RestTimeService {
  private apiUrl = 'http://localhost:5231/api/DoctorRest';

  constructor(private http: HttpClient) { }

  getRestTimes(): Observable<RestTimeInterface[]> {
    return this.http.get<RestTimeInterface[]>(this.apiUrl, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('pms_token')}`
      }
    });
  }

  createRestTime(restTime: RestTimeInterface): Observable<any> {
    return this.http.post<any>(this.apiUrl, restTime, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('pms_token')}`
      }
    });
  }

  updateRestTime(restTime: RestTimeInterface): Observable<any> {
    return this.http.put<any>(this.apiUrl, restTime, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('pms_token')}`
      }
    });
  }

  deleteRestTime(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('pms_token')}`
      }
    });
  }
}
