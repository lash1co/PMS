import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div>
        <h2 class="text-2xl font-semibold text-gray-800">Welcome back, Admin</h2>
        <p class="text-gray-500 mt-1">Here is a quick overview of your clinic today.</p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h3 class="text-gray-500 text-sm font-medium uppercase tracking-wider">Total Patients</h3>
          <p class="text-3xl font-bold text-gray-800 mt-2">1,248</p>
        </div>
        <div class="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h3 class="text-gray-500 text-sm font-medium uppercase tracking-wider">Appointments Today</h3>
          <p class="text-3xl font-bold text-blue-600 mt-2">14</p>
        </div>
        <div class="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <h3 class="text-gray-500 text-sm font-medium uppercase tracking-wider">Pending Invoices</h3>
          <p class="text-3xl font-bold text-orange-500 mt-2">5</p>
        </div>
      </div>
      
      <div class="bg-white p-6 rounded-lg shadow-sm border border-gray-200 h-64 flex items-center justify-center">
        <p class="text-gray-400 italic">Future chart visualization here...</p>
      </div>
    </div>
  `
})
export class DashboardComponent {}