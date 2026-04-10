import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  credentials = { username: '', password: '' };

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

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
  onLogin(): void {
    this.authService.login(this.credentials).subscribe({
      next: (success) => {
        if (success) {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => console.error('Login error', err)
    });
  }
}
