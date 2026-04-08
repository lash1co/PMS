import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Just for testing purposes, this component can be used to verify that the token is being stored correctly after login.
 */
@Component({
  selector: 'app-token',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './token.component.html'
})
export class TokenComponent {
  token = localStorage.getItem('token');
}