import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DoctorService } from '../../services/doctor/doctor.service';
import { ScheduleService } from '../../services/schedule/schedule.service';
import { EncounterService } from '../../services/encounter/encounter.service';
import { DayTimelineComponent } from '../../components/schedule/day-timeline.component';
import { BookingPanelComponent } from '../../components/schedule/booking-panel.component';
import { DoctorSearchComponent } from '../../components/schedule/doctor-search.component';
import { AppointmentEditComponent } from '../../components/schedule/appointment-edit.component';
import { generateDaySlots, slotState, addMinutesToLabel } from '../../utils/schedule-time.util';
import { mockSpecialty } from '../../utils/doctor-specialty.util';
import { DoctorOption } from '../../models/doctor.model';
import { ScheduleView } from '../../models/schedule.model';

interface ConfirmState {
  title: string;
  message: string;
  confirmLabel: string;
  onConfirm: () => void;
}

@Component({
  selector: 'app-schedule',
  standalone: true,
  imports: [CommonModule, DayTimelineComponent, BookingPanelComponent, DoctorSearchComponent, AppointmentEditComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="min-h-screen bg-gray-100 p-4 md:p-8">
      <div class="mx-auto max-w-6xl">
        <!-- Header -->
        <div class="mb-6 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
          <div>
            <h2 class="text-2xl font-semibold text-gray-800">Schedule</h2>
            <p class="text-sm text-gray-500">View a doctor's day and book appointments.</p>
          </div>

          <div class="flex flex-col gap-3 sm:flex-row sm:items-start">
            <div class="w-full sm:w-72">
              <app-doctor-search
                [doctors]="doctors()"
                [selected]="selectedDoctor()"
                (doctorSelected)="selectDoctor($event)"
                (cleared)="clearDoctor()" />
            </div>

            <div class="flex items-center gap-1">
              <button (click)="changeDate(-1)" class="rounded-md border border-gray-300 px-2 py-2 text-sm text-gray-600 hover:bg-gray-50" aria-label="Previous day">‹</button>
              <input type="date" [value]="dateInputValue()" (change)="onDateInput($event)"
                     class="rounded-md border border-gray-300 px-2 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500" />
              <button (click)="changeDate(1)" class="rounded-md border border-gray-300 px-2 py-2 text-sm text-gray-600 hover:bg-gray-50" aria-label="Next day">›</button>
              <button (click)="goToday()" class="rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-600 hover:bg-gray-50">Today</button>
            </div>
          </div>
        </div>

        <!-- Body -->
        @if (selectedDoctor(); as doc) {
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-sm font-medium text-gray-600">{{ doc.name }} · {{ dateInputValue() }}</h3>
            <button (click)="openBooking(null)" [disabled]="availableSlots().length === 0"
                    class="rounded-md bg-blue-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50">
              + New appointment
            </button>
          </div>

          <div class="grid gap-4 lg:grid-cols-[1fr_24rem]">
            <div class="max-h-[72vh] overflow-auto rounded-lg border border-gray-200 bg-white">
              <app-day-timeline
                [events]="scheduleService.events()"
                [date]="scheduleService.selectedDate()"
                [loading]="scheduleService.isLoading()"
                (slotSelected)="openBooking($event)"
                (startEncounter)="onStartEncounter($event)"
                (reschedule)="onRescheduleRequest($event)"
                (cancel)="onCancel($event)"
                (dropBlocked)="showToast('error', $event)"
                (editRequested)="openEdit($event)" />
            </div>

            @if (bookingOpen()) {
              <div class="fixed inset-0 z-30 bg-black/30 lg:hidden" (click)="closeBooking()"></div>
              <div class="fixed inset-x-0 bottom-0 z-40 max-h-[80vh] lg:static lg:z-auto lg:max-h-none">
                <app-booking-panel
                  [doctorId]="doc.id"
                  [date]="scheduleService.selectedDate()"
                  [availableSlots]="availableSlots()"
                  [initialSlot]="initialSlot()"
                  (booked)="onBooked()"
                  (closed)="closeBooking()" />
              </div>
            } @else {
              <div class="hidden rounded-lg border border-dashed border-gray-300 p-6 text-sm text-gray-400 lg:block">
                Click a free slot, or use “New appointment”, to book.
              </div>
            }
          </div>
        } @else {
          <div class="rounded-lg border border-dashed border-gray-300 bg-white p-12 text-center text-gray-500">
            Select a doctor to view their schedule.
          </div>
        }
      </div>

      <!-- Edit appointment dialog -->
      @if (editing(); as appt) {
        <app-appointment-edit
          [appointment]="appt"
          [availableSlots]="availableSlots()"
          (save)="onEditSave($event)"
          (cancel)="closeEdit()" />
      }

      <!-- Confirm dialog -->
      @if (confirmState(); as c) {
        <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" (click)="closeConfirm()">
          <div class="w-full max-w-sm rounded-lg bg-white shadow-xl" (click)="$event.stopPropagation()">
            <div class="px-5 py-4">
              <h3 class="text-sm font-semibold text-gray-800">{{ c.title }}</h3>
              <p class="mt-2 text-sm text-gray-600">{{ c.message }}</p>
            </div>
            <div class="flex justify-end gap-2 border-t border-gray-200 px-5 py-3">
              <button (click)="closeConfirm()" class="rounded-md border border-gray-300 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50">Cancel</button>
              <button (click)="c.onConfirm()" class="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700">{{ c.confirmLabel }}</button>
            </div>
          </div>
        </div>
      }

      <!-- Toast -->
      @if (toast(); as t) {
        <div class="fixed bottom-4 right-4 z-50 flex items-center gap-3 rounded-md px-4 py-3 text-sm text-white shadow-lg"
             [class.bg-green-600]="t.type === 'success'"
             [class.bg-red-600]="t.type === 'error'">
          <span>{{ t.text }}</span>
          @if (t.action; as a) {
            <button (click)="a.run()" class="font-semibold underline">{{ a.label }}</button>
          }
        </div>
      }
    </div>
  `,
})
export class ScheduleComponent implements OnInit {
  readonly scheduleService = inject(ScheduleService);
  private readonly doctorService = inject(DoctorService);
  private readonly encounterService = inject(EncounterService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly doctors = signal<DoctorOption[]>([]);
  readonly selectedDoctor = signal<DoctorOption | null>(null);
  readonly bookingOpen = signal(false);
  readonly initialSlot = signal<string | null>(null);
  readonly toast = signal<{ type: 'success' | 'error'; text: string; action?: { label: string; run: () => void } } | null>(null);
  readonly editing = signal<ScheduleView | null>(null);
  readonly confirmState = signal<ConfirmState | null>(null);

  readonly dateInputValue = computed(() => toLocalDateKey(this.scheduleService.selectedDate()));

  /** Free 'HH:mm' slots for the selected day (not busy, not past). */
  readonly availableSlots = computed(() => {
    const events = this.scheduleService.events();
    const date = this.scheduleService.selectedDate();
    const now = new Date();
    return generateDaySlots()
      .filter((s) => slotState(events, s.minutes, date, now) === 'free')
      .map((s) => s.label);
  });

  ngOnInit(): void {
    const qp = this.route.snapshot.queryParamMap;

    const dateParam = qp.get('date');
    if (dateParam) {
      const [y, m, d] = dateParam.split('-').map(Number);
      if (y && m && d) this.scheduleService.selectedDate.set(new Date(y, m - 1, d, 12, 0, 0));
    } else {
      // Default to tomorrow so the first view lands on a fully bookable day.
      const t = new Date();
      this.scheduleService.selectedDate.set(new Date(t.getFullYear(), t.getMonth(), t.getDate() + 1, 12, 0, 0));
    }

    this.doctorService.searchDoctors('').subscribe({
      next: (list: DoctorOption[]) => {
        const withSpecialty = (list ?? []).map((d) => ({ ...d, specialty: mockSpecialty(d.id) }));
        this.doctors.set(withSpecialty);
        const docParam = qp.get('doctorId');
        if (docParam) {
          const found = withSpecialty.find((x) => x.id === Number(docParam));
          if (found) this.selectDoctor(found);
        }
      },
      error: () => this.doctors.set([]),
    });
  }

  selectDoctor(doctor: DoctorOption): void {
    this.selectedDoctor.set(doctor);
    this.scheduleService.selectedDoctorId.set(doctor.id);
    this.closeBooking();
    this.syncQuery();
  }

  clearDoctor(): void {
    this.selectedDoctor.set(null);
    this.scheduleService.selectedDoctorId.set(null);
    this.closeBooking();
    this.syncQuery();
  }

  changeDate(deltaDays: number): void {
    const current = this.scheduleService.selectedDate();
    this.scheduleService.selectedDate.set(new Date(current.getFullYear(), current.getMonth(), current.getDate() + deltaDays, 12, 0, 0));
    this.closeBooking();
    this.syncQuery();
  }

  goToday(): void {
    const now = new Date();
    this.scheduleService.selectedDate.set(new Date(now.getFullYear(), now.getMonth(), now.getDate(), 12, 0, 0));
    this.closeBooking();
    this.syncQuery();
  }

  onDateInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    if (!value) return;
    const [y, m, d] = value.split('-').map(Number);
    this.scheduleService.selectedDate.set(new Date(y, m - 1, d, 12, 0, 0));
    this.closeBooking();
    this.syncQuery();
  }

  openBooking(slot: string | null): void {
    this.initialSlot.set(slot);
    this.bookingOpen.set(true);
  }

  closeBooking(): void {
    this.bookingOpen.set(false);
    this.initialSlot.set(null);
  }

  onBooked(): void {
    this.closeBooking();
    this.scheduleService.loadSchedule();
    this.showToast('success', 'Appointment booked.');
  }

  onStartEncounter(appointmentId: number): void {
    this.encounterService.startEncounter(appointmentId).subscribe({
      next: () => {
        this.scheduleService.loadSchedule();
        this.showToast('success', 'Encounter started.');
      },
      error: () => this.showToast('error', 'Could not start the encounter.'),
    });
  }

  /** Drag or edit requests a time change → confirm before applying. */
  onRescheduleRequest(event: { id: number; start: string }): void {
    const end = addMinutesToLabel(event.start, 30);
    this.askConfirm('Move appointment', `Move this appointment to ${event.start} – ${end}?`, 'Move', () =>
      this.performReschedule(event.id, event.start),
    );
  }

  private performReschedule(id: number, start: string): void {
    const dateKey = this.dateInputValue();
    const startIso = `${dateKey}T${start}:00`;
    const endIso = `${dateKey}T${addMinutesToLabel(start, 30)}:00`;
    this.scheduleService.rescheduleAppointment(id, startIso, endIso).subscribe({
      next: () => {
        this.scheduleService.loadSchedule();
        this.showToast('success', 'Appointment moved.');
      },
      error: (err) => {
        this.scheduleService.loadSchedule();
        this.showToast('error', typeof err?.error === 'string' ? err.error : 'Could not move the appointment.');
      },
    });
  }

  openEdit(appointment: ScheduleView): void {
    this.editing.set(appointment);
  }

  closeEdit(): void {
    this.editing.set(null);
  }

  onEditSave(event: { id: number; start: string }): void {
    this.closeEdit();
    this.onRescheduleRequest(event);
  }

  private askConfirm(title: string, message: string, confirmLabel: string, onConfirm: () => void): void {
    this.confirmState.set({ title, message, confirmLabel, onConfirm: () => { this.closeConfirm(); onConfirm(); } });
  }

  closeConfirm(): void {
    this.confirmState.set(null);
  }

  onCancel(appointmentId: number): void {
    this.scheduleService.cancelAppointment(appointmentId).subscribe({
      next: () => {
        this.scheduleService.loadSchedule();
        this.showToast('success', 'Appointment cancelled.', { label: 'Undo', run: () => this.undoCancel(appointmentId) });
      },
      error: () => this.showToast('error', 'Could not cancel the appointment.'),
    });
  }

  private undoCancel(appointmentId: number): void {
    this.scheduleService.reactivateAppointment(appointmentId).subscribe({
      next: () => {
        this.scheduleService.loadSchedule();
        this.showToast('success', 'Appointment restored.');
      },
      error: () => this.showToast('error', 'Could not restore the appointment.'),
    });
  }

  private syncQuery(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { doctorId: this.selectedDoctor()?.id ?? null, date: this.dateInputValue() },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  showToast(type: 'success' | 'error', text: string, action?: { label: string; run: () => void }): void {
    this.toast.set({ type, text, action });
    setTimeout(() => this.toast.set(null), action ? 6000 : 3500);
  }
}

function toLocalDateKey(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}
