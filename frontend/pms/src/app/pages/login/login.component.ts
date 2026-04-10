import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  username = '';
  password = '';

  constructor(private http: HttpClient, private router: Router) {}

  signIn() {
    const body = {
      username: this.username,
      password: this.password
    };

this.http.post<{ token: string }>(
  'http://localhost:5231/api/login',
  {
    username: this.username,
    password: this.password
  }
).subscribe({
  next: response => {
    localStorage.setItem('token', response.token);
    this.router.navigate(['/main-menu']);
  },
  error: err => {
    console.log(err);
    alert("Login failed: " + err.error.message);
  }
});
  }
}
