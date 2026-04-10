import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private http: HttpClient) {}

  login(credentials: any): Observable<boolean> {
    // this.http.post<any>('BACKEND/login', credentials)


    /// Login Sim ///
    
    console.log('Sim with:', credentials);
    
    localStorage.setItem('pms_token', 'token_falso_para_demo');
    
    return of(true); 
  }

  logout(): void {
    localStorage.removeItem('pms_token');
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('pms_token');
  }
}