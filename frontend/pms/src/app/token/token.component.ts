import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-token',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './token.component.html'
})
export class TokenComponent {
  token = localStorage.getItem('token');
}