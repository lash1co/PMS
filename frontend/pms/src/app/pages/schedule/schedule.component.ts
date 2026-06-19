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
import { generateDaySlots, slotState, addMinutesToLabel, timeFromIso } from '../../utils/schedule-time.util';
import { DoctorOption } from '../../models/doctor.model';
import { ScheduleView } from '../../models/schedule.model';
import { ConfirmState } from '../../models/confirm-dialog.model';

@Component({
  selector: 'app-schedule',
  standalone: true,
  imports: [CommonModule, DayTimelineComponent, BookingPanelComponent, DoctorSearchComponent, AppointmentEditComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './schedule.component.scss',
  template: `
    <div class="sch">
      <!-- Page header: icon + title + today's date + primary action -->
      <header class="sch-header">
        <div class="sch-header-left">
          <span class="material-icons sch-header-icon">calendar_month</span>
          <div>
            <h1 class="ui-page-title">
              Schedule
              <span class="sch-header-date">· {{ today | date: 'EEE, MMM d' }}</span>
            </h1>
            <p class="ui-page-subtitle">View a doctor's day and book appointments.</p>
          </div>
        </div>
        @if (timelineDoctor()) {
          <button class="ui-btn ui-btn-primary" (click)="openBooking(null)" [disabled]="availableSlots().length === 0">
            <span class="material-icons text-lg">add</span>
            New appointment
          </button>
        }
      </header>

      <!-- Controls -->
      <div class="ui-card sch-controls">
        <div class="sch-controls-search">
          <app-doctor-search
            [doctors]="doctors()"
            [selected]="selectedDoctor()"
            (doctorSelected)="selectDoctor($event)"
            (cleared)="clearDoctor()" />
        </div>

        <div class="sch-datenav">
          <button class="ui-icon-btn" (click)="changeDate(-1)" aria-label="Previous day">
            <span class="material-icons text-lg">chevron_left</span>
          </button>
          <input type="date" class="ui-input" [value]="dateInputValue()" (change)="onDateInput($event)" />
          <button class="ui-icon-btn" (click)="changeDate(1)" aria-label="Next day">
            <span class="material-icons text-lg">chevron_right</span>
          </button>
          <button class="ui-btn ui-btn-outline" (click)="goToday()">Today</button>
        </div>
      </div>

      <!-- Body -->
      @if (timelineDoctor(); as doc) {
        <div class="sch-grid">
          <section class="ui-card sch-timeline-card">
            <div class="sch-tl-card-head">
              <div>
                <span class="sch-tl-section-label">
                  @if (isDefaultView()) { Next available · {{ doc.specialty || 'General Medicine' }} } @else { Day Timeline }
                </span>
                <h2 class="ui-card-title">{{ doc.name }}</h2>
                @if (doc.specialty) { <span class="ui-card-meta">{{ doc.specialty }}</span> }
                @if (isDefaultView()) {
                  <p class="sch-tl-default-hint">No doctor selected — showing the next available {{ doc.specialty || 'General Medicine' }} slot. Search above to view a specific schedule.</p>
                }
              </div>
              <div class="sch-tl-head-right">
                @let s = daySummary();
                <div class="sch-tl-head-stats">
                  <span class="sch-tl-stat">
                    <span class="sch-tl-stat-n">{{ s.total }}</span>
                    <span class="sch-tl-stat-cap">booked</span>
                  </span>
                  <span class="sch-tl-stat">
                    <span class="sch-tl-stat-n sch-tl-stat-n--free">{{ s.free }}</span>
                    <span class="sch-tl-stat-cap">free</span>
                  </span>
                </div>
                <div class="sch-tl-date-badge">
                  <span class="material-icons" style="font-size:1rem;line-height:1;">event</span>
                  {{ scheduleService.selectedDate() | date: 'EEE, MMM d' }}
                </div>
              </div>
            </div>
            <div class="sch-timeline-scroll">
              <app-day-timeline
                [events]="scheduleService.events()"
                [date]="scheduleService.selectedDate()"
                [loading]="scheduleService.isLoading()"
                (slotSelected)="openBooking($event)"
                (reschedule)="onRescheduleRequest($event)"
                (dropBlocked)="showToast('error', $event)"
                (editRequested)="openEdit($event)" />
            </div>
          </section>

          <aside class="sch-side">
            <!-- Next available slots for the selected doctor -->
            <div class="ui-card sch-na">
              <div class="sch-na-head">
                <h3 class="ui-card-title">Next available</h3>
                <span class="ui-pill">{{ doc.name.split(' ')[0] }}</span>
              </div>
              <ul class="sch-na-list">
                @for (slot of nextSlots(); track slot.key) {
                  <li class="sch-na-item" (click)="jumpToSlot(slot)">
                    <span class="sch-na-date">{{ slot.dateLabel }}</span>
                    <span class="sch-na-time">
                      <span class="material-icons" style="font-size:0.75rem;line-height:1;">schedule</span>
                      {{ slot.time }}
                    </span>
                  </li>
                }
              </ul>
            </div>

            @if (bookingOpen()) {
              <div class="sch-side-backdrop" (click)="closeBooking()"></div>
              <div class="sch-side-sheet">
                <app-booking-panel
                  [doctorId]="doc.id"
                  [date]="scheduleService.selectedDate()"
                  [availableSlots]="availableSlots()"
                  [initialSlot]="initialSlot()"
                  (booked)="onBooked()"
                  (closed)="closeBooking()" />
              </div>
            } @else {
              <div class="ui-card ui-card-pad sch-side-empty">
                @let s = daySummary();
                <h3 class="ui-card-title">Day overview</h3>
                <div class="sch-summary-stats">
                  <div class="sch-summary-stat">
                    <span class="sch-summary-num">{{ s.total }}</span>
                    <span class="sch-summary-cap">Booked</span>
                  </div>
                  <div class="sch-summary-stat">
                    <span class="sch-summary-num">{{ s.free }}</span>
                    <span class="sch-summary-cap">Free slots</span>
                  </div>
                </div>

                <ul class="sch-legend">
                  <li><span class="sch-legend-dot sch-legend-dot--scheduled"></span> Scheduled <span class="sch-legend-n">{{ s.scheduled }}</span></li>
                  <li><span class="sch-legend-dot sch-legend-dot--inprogress"></span> In progress <span class="sch-legend-n">{{ s.inProgress }}</span></li>
                  <li><span class="sch-legend-dot sch-legend-dot--completed"></span> Completed <span class="sch-legend-n">{{ s.completed }}</span></li>
                  <li>
                    <span class="sch-legend-dot sch-legend-dot--rest"></span>
                    Rest period
                    <span class="sch-legend-n">{{ s.restRange ?? '—' }}</span>
                  </li>
                </ul>

                <p class="sch-side-tip">Click a free slot, or use "New appointment", to book.</p>
              </div>
            }

          </aside>

          <!-- General Medicine quick access — 3rd grid column on desktop -->
          <div class="sch-qp sch-qp-ref sch-qp-col">
            <div class="sch-qp-head">
              <h3 class="sch-qp-ref-title">General Medicine</h3>
              <span class="sch-qp-ref-badge">Shortcut</span>
            </div>
            @if (quickGPs().length === 0) {
              <p class="sch-qp-empty">No General Medicine doctors available.</p>
            } @else {
              <ul class="sch-qp-list">
                @for (gp of quickGPs(); track gp.id) {
                  <li class="sch-qp-item" [class.sch-qp-item--active]="doc.id === gp.id"
                      (click)="selectDoctor(gp)">
                    <span class="sch-qp-name">{{ gp.name }}</span>
                    <span class="sch-qp-slot">
                      <span class="material-icons" style="font-size:0.875rem;line-height:1;">schedule</span>
                      {{ gp.nextSlot }}
                    </span>
                  </li>
                }
              </ul>
            }
          </div>
        </div>
      } @else {
        <div class="ui-card sch-empty">
          Select a doctor to view their schedule.
        </div>
      }

      <!-- Edit appointment dialog -->
      @if (editing(); as appt) {
        <app-appointment-edit
          [appointment]="appt"
          [availableSlots]="availableSlots()"
          (save)="onEditSave($event)"
          (startEncounter)="onEditStartEncounter($event)"
          (cancelAppointment)="onEditCancel($event)"
          (cancel)="closeEdit()" />
      }

      <!-- Confirm dialog -->
      @if (confirmState(); as c) {
        <div class="ui-modal-backdrop" (click)="closeConfirm()">
          <div class="ui-modal" (click)="$event.stopPropagation()">
            <div class="ui-modal-body">
              <h3 class="ui-modal-title">{{ c.title }}</h3>
              <p class="text-sm" style="color: var(--pms-muted); margin:0;">{{ c.message }}</p>
            </div>
            <div class="ui-modal-foot">
              <button class="ui-btn ui-btn-outline" (click)="closeConfirm()">Cancel</button>
              <button class="ui-btn ui-btn-primary" (click)="c.onConfirm()">{{ c.confirmLabel }}</button>
            </div>
          </div>
        </div>
      }

      <!-- Toast -->
      @if (toast(); as t) {
        <div class="sch-toast" [class.sch-toast--success]="t.type === 'success'" [class.sch-toast--error]="t.type === 'error'">
          <span>{{ t.text }}</span>
          @if (t.action; as a) {
            <button class="sch-toast-action" (click)="a.run()">{{ a.label }}</button>
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

  readonly today = new Date();

  readonly doctors = signal<DoctorOption[]>([]);
  readonly selectedDoctor = signal<DoctorOption | null>(null);
  readonly defaultDoctor = signal<DoctorOption | null>(null);
  readonly isDefaultView = signal(false);
  /** The doctor whose schedule the timeline shows. Null selectedDoctor falls back to the default GM doctor. */
  readonly timelineDoctor = computed(() => this.selectedDoctor() ?? this.defaultDoctor());
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

  /** Next 4 available slots for the timeline doctor across upcoming days (mock). */
  readonly nextSlots = computed(() => {
    const doc = this.timelineDoctor();
    if (!doc) return [];
    const TIMES = ['08:00', '09:30', '10:00', '11:30', '14:00', '15:00', '16:30'];
    const base = new Date();
    return Array.from({ length: 4 }, (_, i) => {
      const d = new Date(base.getFullYear(), base.getMonth(), base.getDate() + i);
      const time = TIMES[(Math.abs(doc.id) * 3 + i) % TIMES.length];
      const dateLabel = i === 0 ? 'Today' : i === 1 ? 'Tomorrow'
        : d.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' });
      return { key: `${d.toDateString()}-${time}`, dateLabel, time, date: d };
    });
  });

  /** General Medicine doctors + a deterministic mock next-available slot. */
  readonly quickGPs = computed(() => {
    const MOCK_SLOTS = ['08:00', '08:30', '09:00', '09:30', '10:00', '10:30'];
    return this.doctors()
      .filter(d => d.specialty === 'General Medicine')
      .slice(0, 4)
      .map(d => ({ ...d, nextSlot: MOCK_SLOTS[Math.abs(d.id) % MOCK_SLOTS.length] }));
  });

  /** Per-status counts + rest range for the side day-summary. */
  readonly daySummary = computed(() => {
    const all = this.scheduleService.events();
    const appts = all.filter((e) => e.type !== 'Rest');
    const rests = all.filter((e) => e.type === 'Rest');
    const restRange = rests.length > 0
      ? rests.map(r => `${timeFromIso(r.startTime)} – ${timeFromIso(r.endTime)}`).join(', ')
      : null;
    return {
      total: appts.length,
      scheduled: appts.filter((e) => e.scheduleStatus === 'Scheduled').length,
      inProgress: appts.filter((e) => e.scheduleStatus === 'InProgress').length,
      completed: appts.filter((e) => e.scheduleStatus === 'Completed').length,
      free: this.availableSlots().length,
      restRange,
    };
  });

  ngOnInit(): void {
    const qp = this.route.snapshot.queryParamMap;

    const dateParam = qp.get('date');
    if (dateParam) {
      const [y, m, d] = dateParam.split('-').map(Number);
      if (y && m && d) this.scheduleService.selectedDate.set(new Date(y, m - 1, d, 12, 0, 0));
    } else {
      const t = new Date();
      this.scheduleService.selectedDate.set(new Date(t.getFullYear(), t.getMonth(), t.getDate(), 12, 0, 0));
    }

    this.doctorService.searchDoctors('').subscribe({
      next: (list: DoctorOption[]) => {
        const doctors = list ?? [];
        this.doctors.set(doctors);

        // Prefer a GM doctor as default; fall back to the first doctor available.
        const fallback = doctors.find(d => d.specialty === 'General Medicine') ?? doctors[0];
        if (fallback) this.defaultDoctor.set(fallback);

        const docParam = qp.get('doctorId');
        if (docParam) {
          const found = doctors.find((x) => x.id === Number(docParam));
          if (found) this.selectDoctor(found);
        } else if (fallback) {
          this.isDefaultView.set(true);
          // Keep selectedDoctor null so the search bar appears empty in default view.
          this.scheduleService.selectedDoctorId.set(fallback.id);
        }
      },
      error: () => this.doctors.set([]),
    });
  }

  selectDoctor(doctor: DoctorOption): void {
    this.isDefaultView.set(false);
    this.selectedDoctor.set(doctor);
    this.scheduleService.selectedDoctorId.set(doctor.id);
    this.closeBooking();
    this.syncQuery();
  }

  clearDoctor(): void {
    const def = this.defaultDoctor();
    this.isDefaultView.set(true);
    this.selectedDoctor.set(null);
    this.scheduleService.selectedDoctorId.set(def?.id ?? null);
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

  jumpToSlot(slot: { date: Date; time: string }): void {
    this.scheduleService.selectedDate.set(
      new Date(slot.date.getFullYear(), slot.date.getMonth(), slot.date.getDate(), 12, 0, 0)
    );
    this.syncQuery();
    this.openBooking(slot.time);
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

  onEditStartEncounter(id: number): void {
    this.closeEdit();
    this.onStartEncounter(id);
  }

  onEditCancel(id: number): void {
    this.closeEdit();
    this.onCancel(id);
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
      queryParams: {
        doctorId: this.isDefaultView() ? null : (this.selectedDoctor()?.id ?? null),
        date: this.dateInputValue(),
      },
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
