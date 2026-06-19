import { Component, ChangeDetectionStrategy, inject, input, output, signal, computed, linkedSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PatientService } from '../../services/patient/patient.service';
import { ScheduleService } from '../../services/schedule/schedule.service';
import { Patient } from '../../models/patient.model';
import { addMinutesToLabel } from '../../utils/schedule-time.util';

/**
 * Booking surface for a single 30-min slot: choose the time (from free slots),
 * pick a patient + reason and confirm. End time is fixed at start + 30 min.
 */
@Component({
  selector: 'app-booking-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex h-full flex-col rounded-lg border border-gray-200 bg-white shadow-sm">
      <div class="flex items-center justify-between border-b border-gray-200 px-4 py-3">
        <h3 class="text-sm font-semibold text-gray-800">Book appointment</h3>
        <button (click)="closed.emit()" class="text-gray-400 hover:text-gray-600" aria-label="Close booking panel">✕</button>
      </div>

      @if (availableSlots().length === 0) {
        <div class="p-6 text-sm text-gray-500">No free slots on this day. Try another date.</div>
      } @else {
        <div class="flex flex-col gap-4 overflow-y-auto p-4">
          <!-- Time -->
          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Time</label>
            <select [ngModel]="selectedTime()" (ngModelChange)="selectedTime.set($event)"
                    class="block w-full rounded-md border border-gray-300 p-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
              @for (slot of availableSlots(); track slot) {
                <option [value]="slot">{{ slot }} – {{ endFor(slot) }}</option>
              }
            </select>
            <p class="mt-1 text-xs text-gray-400">Fixed 30-minute appointment.</p>
          </div>

          <!-- Patient -->
          <div class="relative">
            <label class="mb-1 block text-sm font-medium text-gray-700">Patient</label>
            @if (selectedPatient(); as p) {
              <div class="flex items-center justify-between rounded-md border border-blue-200 bg-blue-50 px-3 py-2">
                <span class="text-sm text-blue-900">{{ p.firstName }} {{ p.lastName }}</span>
                <button (click)="clearPatient()" class="text-blue-500 hover:text-blue-700" aria-label="Clear patient">✕</button>
              </div>
            } @else {
              <input type="text" [(ngModel)]="patientTerm" (ngModelChange)="onSearch($event)"
                     placeholder="Search by name or ID…"
                     class="block w-full rounded-md border border-gray-300 p-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500" />
              @if (patientResults().length > 0) {
                <div class="absolute z-10 mt-1 max-h-48 w-full overflow-y-auto rounded-md border border-gray-200 bg-white shadow-lg">
                  @for (p of patientResults(); track p.id) {
                    <button (click)="selectPatient(p)"
                            class="block w-full border-b border-gray-100 px-3 py-2 text-left text-sm last:border-0 hover:bg-blue-50">
                      <strong>{{ p.firstName }} {{ p.lastName }}</strong>
                      <span class="ml-2 text-gray-500">(ID: {{ p.id }})</span>
                    </button>
                  }
                </div>
              }
            }
          </div>

          <!-- Reason -->
          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Reason for visit</label>
            <textarea [(ngModel)]="reason" rows="3" placeholder="e.g., Routine checkup…"
                      class="block w-full rounded-md border border-gray-300 p-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"></textarea>
          </div>

          @if (errorMessage()) {
            <div class="rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ errorMessage() }}</div>
          }
        </div>

        <div class="mt-auto border-t border-gray-200 p-4">
          <button (click)="save()" [disabled]="!canSave()"
                  class="w-full rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50">
            {{ isSaving() ? 'Booking…' : 'Book appointment' }}
          </button>
        </div>
      }
    </div>
  `,
})
export class BookingPanelComponent {
  private readonly patientService = inject(PatientService);
  private readonly scheduleService = inject(ScheduleService);

  readonly doctorId = input.required<number>();
  readonly date = input.required<Date>();
  readonly availableSlots = input<string[]>([]); // free 'HH:mm' labels
  readonly initialSlot = input<string | null>(null);

  readonly booked = output<void>();
  readonly closed = output<void>();

  /** Selected start time; defaults to the requested slot or the first free one, resets when inputs change. */
  readonly selectedTime = linkedSignal<string>(() => this.initialSlot() ?? this.availableSlots()[0] ?? '');

  readonly patientTerm = signal('');
  readonly patientResults = signal<Patient[]>([]);
  readonly selectedPatient = signal<Patient | null>(null);
  readonly reason = signal('');
  readonly isSaving = signal(false);
  readonly errorMessage = signal('');

  readonly canSave = computed(
    () => !!this.selectedTime() && !!this.selectedPatient() && this.reason().trim().length > 0 && !this.isSaving(),
  );

  endFor(slot: string): string {
    return addMinutesToLabel(slot, 30);
  }

  onSearch(term: string): void {
    if (!term || term.trim().length < 1) {
      this.patientResults.set([]);
      return;
    }
    this.patientService.searchPatients(term.trim()).subscribe({
      next: (results) => this.patientResults.set(results ?? []),
      error: () => this.patientResults.set([]),
    });
  }

  selectPatient(patient: Patient): void {
    this.selectedPatient.set(patient);
    this.patientResults.set([]);
    this.patientTerm.set('');
  }

  clearPatient(): void {
    this.selectedPatient.set(null);
  }

  save(): void {
    const patient = this.selectedPatient();
    const time = this.selectedTime();
    if (!patient?.id || !time || !this.canSave()) return;

    this.isSaving.set(true);
    this.errorMessage.set('');

    const dateKey = toLocalDateKey(this.date());
    const start = `${dateKey}T${time}:00`;
    const end = `${dateKey}T${addMinutesToLabel(time, 30)}:00`;

    this.scheduleService
      .createAppointment({ startTime: start, endTime: end, reason: this.reason().trim(), doctorId: this.doctorId(), patientId: patient.id })
      .subscribe({
        next: () => {
          this.isSaving.set(false);
          this.booked.emit();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.errorMessage.set(typeof err?.error === 'string' ? err.error : 'Could not book the appointment. Please try another slot.');
        },
      });
  }
}

function toLocalDateKey(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}
