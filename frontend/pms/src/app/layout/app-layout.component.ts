import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="flex h-screen bg-gray-100">

      <aside class="w-64 bg-gray-900 text-white flex flex-col shadow-lg">
        <div class="h-16 flex items-center justify-center border-b border-gray-800">
          <h1 class="text-xl font-bold tracking-wider">PMS Admin</h1>
        </div>

        <nav class="flex-1 px-4 py-6 space-y-2">

          <a routerLink="/dashboard" routerLinkActive="bg-gray-800 border-l-4 border-blue-500"
             class="flex items-center px-4 py-3 text-gray-300 hover:bg-gray-800 hover:text-white rounded-r-md transition-colors">
             <span class="font-medium">Dashboard</span>
          </a>

          <a routerLink="/users" routerLinkActive="bg-gray-800 border-l-4 border-blue-500"
             class="flex items-center px-4 py-3 text-gray-300 hover:bg-gray-800 hover:text-white rounded-r-md transition-colors">
             <span class="font-medium">Users Management</span>
          </a>

          <a routerLink="/patients" routerLinkActive="bg-gray-800 border-l-4 border-blue-500"
             class="flex items-center px-4 py-3 text-gray-300 hover:bg-gray-800 hover:text-white rounded-r-md transition-colors">
             <span class="font-medium">Patients Management</span>
          </a>

          <a routerLink="/schedule" routerLinkActive="bg-gray-800 border-l-4 border-blue-500"
             class="flex items-center px-4 py-3 text-gray-300 hover:bg-gray-800 hover:text-white rounded-r-md transition-colors">
             <span class="font-medium">Schedule</span>
          </a>

          <a routerLink="doctor-rest-time" routerLinkActive="bg-gray-800 border-l-4 border-blue-500"
             class="flex items-center px-4 py-3 text-gray-300 hover:bg-gray-800 hover:text-white rounded-r-md transition-colors">
             <span class="font-medium">Doctor Rest Time</span>
          </a>

          <a routerLink="/encounters" routerLinkActive="bg-gray-800 border-l-4 border-blue-500"
             class="flex items-center px-4 py-3 text-gray-300 hover:bg-gray-800 hover:text-white rounded-r-md transition-colors">
             <span class="font-medium">Encounters</span>
          </a>
        </nav>

        <div class="p-4 border-t border-gray-800">
          <div class="flex items-center space-x-3 mb-4">
            <div class="w-8 h-8 rounded-full bg-blue-500 flex items-center justify-center text-white font-bold">
              U </div>
            <div class="text-sm">
              <p class="font-medium text-white">{{ userName }}</p>
              <p class="text-gray-400 text-xs">{{ user }}</p>
            </div>
          </div>
          <button (click)="onLogout()" class="w-full text-left text-sm text-gray-400 hover:text-white transition">
            Log out
          </button>
        </div>
      </aside>

      <main class="flex-1 flex flex-col overflow-hidden">
        <header class="h-16 bg-white shadow-sm flex items-center px-8">
          <h2 class="text-lg font-medium text-gray-700">Medical Management System</h2>
        </header>

        <div class="flex-1 overflow-y-auto p-8">
          <router-outlet></router-outlet>
        </div>
      </main>

    </div>
  `
})

export class AppLayoutComponent {
  userName: string = '';
  user: string = '';
  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.userName = localStorage.getItem('pms_user_name') || '';
    this.user = localStorage.getItem('pms_user') || '';
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
