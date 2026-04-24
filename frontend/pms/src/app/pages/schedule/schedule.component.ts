import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { DoctorService } from '../../services/doctor/doctor.service';
import { DailyScheduleModalComponent } from '../../components/daily-schedule-modal.component';

@Component({
  selector: 'app-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule],
  template: `
    <div class="min-h-screen bg-gray-100 p-8">
      <div class="max-w-6xl mx-auto bg-white rounded-lg shadow-sm border border-gray-200 p-8">
        
        <div class="flex flex-col md:flex-row md:items-center md:justify-between mb-8">
          <h2 class="text-2xl font-semibold text-gray-800">Doctor Schedules</h2>
          
          <div class="mt-4 md:mt-0 flex w-full md:w-auto items-center space-x-4">
            <div class="flex w-full md:w-80">
              <input 
                type="text" 
                placeholder="Search doctor by name or ID..." 
                [(ngModel)]="searchTerm" 
                (keyup.enter)="onSearch()"
                class="w-full rounded-l-md border border-gray-300 px-4 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
              >
              <button 
                (click)="onSearch()"
                class="bg-blue-600 text-white px-4 py-2 rounded-r-md text-sm font-medium hover:bg-blue-700 transition"
              >
                Search
              </button>
            </div>
          </div>
        </div>

        <div class="overflow-x-auto border border-gray-200 rounded-lg">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Doctor Name</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              @for (doctor of doctorsList(); track doctor.id) {
                <tr class="hover:bg-gray-50 transition">
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">#{{ doctor.id }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-800">{{ doctor.name }}</td>
                  <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button 
                      (click)="openDailySchedule(doctor)" 
                      class="text-blue-600 hover:text-blue-900 font-semibold border border-blue-600 px-3 py-1 rounded hover:bg-blue-50 transition">
                      View Schedule
                    </button>
                  </td>
                </tr>
              }
              @if (doctorsList().length === 0) {
                <tr>
                  <td colspan="3" class="px-6 py-12 text-center text-gray-500 italic">No doctors found.</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `
})
export class ScheduleComponent implements OnInit {
  doctorService = inject(DoctorService);
  dialog = inject(MatDialog);

  searchTerm = signal<string>('');
  doctorsList = signal<any[]>([]);

  ngOnInit() {
    this.onSearch(); // Carga inicial
  }

  onSearch() {
    this.doctorService.searchDoctors(this.searchTerm()).subscribe({
      next: (data) => this.doctorsList.set(data),
      error: (err) => console.error(err)
    });
  }

  openDailySchedule(doctor: any) {
    this.dialog.open(DailyScheduleModalComponent, {
      width: '800px',
      height: '80vh',
      data: { doctorId: doctor.id, doctorName: doctor.name }
    });
  }
}