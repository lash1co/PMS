import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { ScheduleService } from '../services/schedule/schedule.service';

@Component({
  selector: 'app-appointment-form',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule],
  template: `
    <div class="p-6">
      <h2 class="text-xl font-bold text-gray-800 mb-4" mat-dialog-title>
        Schedule New Appointment
      </h2>
      
      <div mat-dialog-content class="flex flex-col gap-4 mt-2">
        <div>
          <label class="block text-sm font-medium text-gray-700">Date</label>
          <input type="text" [value]="data.date | date:'longDate'" disabled 
                 class="mt-1 block w-full rounded-md border-gray-300 bg-gray-100 p-2 text-gray-500">
        </div>

        <div class="flex gap-4">
          <div class="flex-1">
            <label class="block text-sm font-medium text-gray-700">Start time</label>
            <input type="time" [(ngModel)]="startTime" 
                   class="mt-1 block w-full rounded-md border border-gray-300 p-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
          </div>

          <div class="flex-1">
            <label class="block text-sm font-medium text-gray-700">End time</label>
            <input type="time" [(ngModel)]="endTime" 
                   class="mt-1 block w-full rounded-md border border-gray-300 p-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
          </div>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700">Patient ID</label>
          <input type="number" [(ngModel)]="patientId" placeholder="Ej. 1"
                 class="mt-1 block w-full rounded-md border border-gray-300 p-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700">Reason</label>
          <textarea [(ngModel)]="reason" rows="3" placeholder="Ej. Chequeo general..."
                    class="mt-1 block w-full rounded-md border border-gray-300 p-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"></textarea>
        </div>
        
        @if (errorMessage()) {
          <div class="p-3 bg-red-50 text-red-700 text-sm rounded-md mt-2 border border-red-200">
            {{ errorMessage() }}
          </div>
        }
      </div>

      <div mat-dialog-actions class="flex justify-end gap-3 mt-6">
        <button mat-dialog-close class="px-4 py-2 text-gray-600 bg-gray-200 rounded hover:bg-gray-300 transition font-medium">
          Cancel
        </button>
        <button (click)="saveAppointment()" [disabled]="isSaving()"
                class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition font-medium disabled:opacity-50 disabled:cursor-not-allowed">
          {{ isSaving() ? 'Guardando...' : 'Guardar Cita' }}
        </button>
      </div>
    </div>
  `
})
export class AppointmentFormComponent {
  data = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<AppointmentFormComponent>);
  scheduleService = inject(ScheduleService);

  startTime = signal<string>('09:00');
  endTime = signal<string>('09:30');
  patientId = signal<number | null>(null);
  reason = signal<string>('');
  
  isSaving = signal<boolean>(false);
  errorMessage = signal<string>('');

  saveAppointment() {
    if (!this.patientId() || !this.reason() || !this.startTime() || !this.endTime()) {
      this.errorMessage.set('Por favor llena todos los campos.');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    const dateStr = this.data.date.toISOString().split('T')[0];
    const startDateTime = new Date(`${dateStr}T${this.startTime()}:00`).toISOString();
    const endDateTime = new Date(`${dateStr}T${this.endTime()}:00`).toISOString();

    const newAppointment = {
      startTime: startDateTime,
      endTime: endDateTime,
      status: 'Scheduled',
      reason: this.reason(),
      doctor: { id: this.data.doctorId },
      patient: { id: this.patientId() }
    };

    this.scheduleService.createAppointment(newAppointment).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.dialogRef.close(true); 
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Hubo un error al guardar la cita. Verifica que el ID del paciente exista.');
        this.isSaving.set(false);
      }
    });
  }
}