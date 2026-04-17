import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class UserService {
  private apiUrl = 'http://localhost:5231/api/users';
  private token: string | null = null;

  constructor(private http: HttpClient) {
    this.token = localStorage.getItem('pms_token');
  }

  getUsers(): Observable<UserInterface[]> {
    return this.http.get<UserInterface[]>(this.apiUrl, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('pms_token')}`
      }
    });
  }
}
