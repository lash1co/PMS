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
  styleUrl: './booking-panel.component.scss',
  template: `
    <div class="sch-bp">
      <div class="sch-bp-head">
        <h3 class="ui-card-title">Book appointment</h3>
        <button class="ui-close" (click)="closed.emit()" aria-label="Close booking panel">✕</button>
      </div>

      @if (availableSlots().length === 0) {
        <div class="sch-bp-empty">No free slots on this day. Try another date.</div>
      } @else {
        <div class="sch-bp-body">
          <!-- Time -->
          <div>
            <label class="ui-label">Time</label>
            <select class="ui-select" [ngModel]="selectedTime()" (ngModelChange)="selectedTime.set($event)">
              @for (slot of availableSlots(); track slot) {
                <option [value]="slot">{{ slot }} – {{ endFor(slot) }}</option>
              }
            </select>
            <p class="sch-bp-hint">Fixed 30-minute appointment.</p>
          </div>

          <!-- Patient -->
          <div class="sch-bp-field">
            <label class="ui-label">Patient</label>
            @if (selectedPatient(); as p) {
              <div class="sch-bp-selected">
                <span class="sch-bp-selected-name">{{ p.firstName }} {{ p.lastName }}</span>
                <button class="sch-bp-selected-clear" (click)="clearPatient()" aria-label="Clear patient">✕</button>
              </div>
            } @else {
              <input type="text" class="ui-input" [(ngModel)]="patientTerm" (ngModelChange)="onSearch($event)"
                     placeholder="Search by name or ID…" />
              @if (patientResults().length > 0) {
                <div class="sch-bp-results">
                  @for (p of patientResults(); track p.id) {
                    <button class="sch-bp-result" (click)="selectPatient(p)">
                      <strong>{{ p.firstName }} {{ p.lastName }}</strong>
                      <span class="sch-bp-result-id">(ID: {{ p.id }})</span>
                    </button>
                  }
                </div>
              }
            }
          </div>

          <!-- Reason -->
          <div>
            <label class="ui-label">Reason for visit</label>
            <textarea class="ui-input" [(ngModel)]="reason" rows="3" placeholder="e.g., Routine checkup…"></textarea>
          </div>

          @if (errorMessage()) {
            <div class="sch-bp-error">{{ errorMessage() }}</div>
          }
        </div>

        <div class="sch-bp-foot">
          <button class="ui-btn ui-btn-primary w-full" style="justify-content:center;" (click)="save()" [disabled]="!canSave()">
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
