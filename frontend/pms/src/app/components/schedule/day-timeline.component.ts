import { Component, ChangeDetectionStrategy, input, output, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DragDropModule, CdkDragEnd } from '@angular/cdk/drag-drop';
import { ScheduleView } from '../../models/schedule.model';
import {
  DAY_TOTAL_MINUTES,
  SLOT_MINUTES,
  generateDaySlots,
  minutesFromIso,
  minutesToLabel,
  timeFromIso,
  slotState,
  nowMinutes,
  isMinutesPast,
  isBeforeDay,
} from '../../utils/schedule-time.util';

const PX_PER_MIN = 1.6; // 30-min slot = 48px

interface PositionedEvent {
  ref: ScheduleView;
  top: number;
  height: number;
  isRest: boolean;
  status: string;
  timeLabel: string;
}

/**
 * Proportional day timeline (08:00–18:00, 30-min slots). Empty, non-past slots are
 * clickable to start a booking; appointments render as duration-proportional blocks
 * that can be dragged to another free slot to reschedule.
 */
@Component({
  selector: 'app-day-timeline',
  standalone: true,
  imports: [CommonModule, DragDropModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="relative flex bg-white">
      <!-- Time gutter -->
      <div class="w-16 shrink-0 border-r border-gray-200">
        @for (slot of slots; track slot.minutes) {
          <div class="flex items-start justify-end pr-2 pt-1 text-xs text-gray-400" [style.height.px]="slotHeight">
            {{ slot.label }}
          </div>
        }
      </div>

      <!-- Track -->
      <div class="timeline-track relative flex-1" [style.height.px]="totalHeight">
        @if (loading()) {
          <div class="absolute inset-0 z-30 flex items-center justify-center bg-white/60 text-sm text-gray-500">
            Loading schedule…
          </div>
        }

        <!-- Past region shading -->
        @if (pastHeight() > 0) {
          <div class="pointer-events-none absolute left-0 right-0 top-0 z-0 bg-rose-50/70" [style.height.px]="pastHeight()"></div>
        }

        <!-- Slot backgrounds (booking targets) -->
        @for (slot of slots; track slot.minutes) {
          @let state = stateOf(slot.minutes);
          <button type="button"
                  class="absolute left-0 right-0 flex items-center border-b border-gray-100 px-3 text-left text-xs transition-colors disabled:cursor-default"
                  [style.top.px]="slot.minutes * pxPerMin"
                  [style.height.px]="slotHeight"
                  [disabled]="state !== 'free'"
                  [attr.title]="state === 'past' ? 'Past — cannot book' : null"
                  [class.cursor-pointer]="state === 'free'"
                  [class.hover:bg-blue-50]="state === 'free'"
                  (click)="slotSelected.emit(slot.label)">
            @if (state === 'free') {
              <span class="italic text-gray-300">Free — click to book</span>
            } @else if (state === 'past') {
              <span class="text-[11px] uppercase tracking-wide text-rose-300">Past</span>
            }
          </button>
        }

        <!-- Event blocks -->
        @for (pe of positionedEvents(); track pe.ref.type + '-' + pe.ref.id) {
          <div class="absolute left-1 right-2 z-10 overflow-hidden rounded-md border px-3 py-1.5 shadow-sm"
               [style.top.px]="pe.top"
               [style.height.px]="pe.height"
               [ngClass]="blockClasses(pe)"
               cdkDrag
               cdkDragLockAxis="y"
               cdkDragBoundary=".timeline-track"
               [cdkDragDisabled]="!isDraggable(pe)"
               [class.cursor-move]="isDraggable(pe)"
               (cdkDragStarted)="onDragStarted()"
               (cdkDragEnded)="onDragEnded($event, pe)"
               (click)="onBlockClick(pe)">
            <div class="flex items-start justify-between gap-2">
              <span class="text-xs font-semibold">{{ pe.timeLabel }}</span>
              @if (pe.isRest) {
                <span class="text-[11px] uppercase tracking-wide text-slate-500">Rest</span>
              } @else {
                <span class="text-[11px] uppercase tracking-wide" [ngClass]="statusTextClass(pe.status)">{{ pe.status }}</span>
              }
            </div>

            @if (pe.isRest) {
              <p class="mt-0.5 truncate text-xs text-slate-600">{{ pe.ref.scheduleDescription }}</p>
            } @else {
              <p class="mt-0.5 truncate text-xs text-gray-800">
                <strong>{{ pe.ref.patientName }}</strong> · {{ pe.ref.scheduleDescription }}
              </p>

              @if (pe.status === 'Scheduled') {
                @if (confirmingId() === pe.ref.id) {
                  <div class="mt-1 flex gap-2">
                    <button (click)="$event.stopPropagation(); confirmStart(pe.ref.id)"
                            class="rounded bg-blue-600 px-2 py-0.5 text-xs font-semibold text-white hover:bg-blue-700">Confirm start</button>
                    <button (click)="$event.stopPropagation(); confirmingId.set(null)"
                            class="rounded border border-gray-300 px-2 py-0.5 text-xs text-gray-600 hover:bg-gray-50">Back</button>
                  </div>
                } @else {
                  <div class="mt-1 flex gap-2">
                    <button (click)="$event.stopPropagation(); confirmingId.set(pe.ref.id)"
                            class="rounded bg-blue-600 px-2 py-0.5 text-xs font-semibold text-white hover:bg-blue-700">Start encounter</button>
                    <button (click)="$event.stopPropagation(); cancel.emit(pe.ref.id)"
                            class="rounded border border-gray-300 px-2 py-0.5 text-xs text-gray-600 hover:bg-gray-50">Cancel</button>
                  </div>
                }
              }
            }
          </div>
        }

        <!-- Current-time indicator -->
        @if (nowOffset() !== null) {
          <div class="absolute left-0 right-0 z-20 border-t-2 border-red-500" [style.top.px]="nowOffset()">
            <span class="absolute -left-1 -top-2 h-2 w-2 rounded-full bg-red-500"></span>
          </div>
        }
      </div>
    </div>
  `,
})
export class DayTimelineComponent {
  readonly events = input<ScheduleView[]>([]);
  readonly date = input.required<Date>();
  readonly loading = input<boolean>(false);

  readonly slotSelected = output<string>();
  readonly startEncounter = output<number>();
  readonly reschedule = output<{ id: number; start: string }>();
  readonly cancel = output<number>();
  readonly dropBlocked = output<string>();
  readonly editRequested = output<ScheduleView>();

  readonly slots = generateDaySlots();
  readonly pxPerMin = PX_PER_MIN;
  readonly slotHeight = SLOT_MINUTES * PX_PER_MIN;
  readonly totalHeight = DAY_TOTAL_MINUTES * PX_PER_MIN;

  readonly confirmingId = signal<number | null>(null);
  private readonly now = signal(new Date());

  readonly positionedEvents = computed<PositionedEvent[]>(() =>
    this.events().map((ref) => {
      const startMin = minutesFromIso(ref.startTime);
      const endMin = minutesFromIso(ref.endTime);
      return {
        ref,
        top: Math.max(0, startMin) * PX_PER_MIN,
        height: Math.max(SLOT_MINUTES, endMin - startMin) * PX_PER_MIN,
        isRest: ref.type === 'Rest',
        status: ref.scheduleStatus,
        timeLabel: `${timeFromIso(ref.startTime)} – ${timeFromIso(ref.endTime)}`,
      };
    }),
  );

  readonly nowOffset = computed<number | null>(() => {
    const m = nowMinutes(this.date(), this.now());
    if (m === null || m < 0 || m > DAY_TOTAL_MINUTES) return null;
    return m * PX_PER_MIN;
  });

  /** Height of the shaded "past" region: full day for past dates, up to now for today, 0 for future. */
  readonly pastHeight = computed<number>(() => {
    const date = this.date();
    const now = this.now();
    if (isBeforeDay(date, now)) return this.totalHeight;
    const m = nowMinutes(date, now);
    if (m === null) return 0; // future day
    return Math.min(Math.max(m, 0), DAY_TOTAL_MINUTES) * PX_PER_MIN;
  });

  stateOf(slotMinutes: number) {
    return slotState(this.events(), slotMinutes, this.date(), this.now());
  }

  isDraggable(pe: PositionedEvent): boolean {
    return !pe.isRest && pe.status === 'Scheduled';
  }

  private wasDragged = false;

  onDragStarted(): void {
    this.wasDragged = true;
  }

  /** Open the edit dialog on a plain click (ignored right after a drag). */
  onBlockClick(pe: PositionedEvent): void {
    if (this.wasDragged) return; // suppress the click that follows a drag
    if (!pe.isRest && pe.status === 'Scheduled') {
      this.editRequested.emit(pe.ref);
    }
  }

  confirmStart(id: number): void {
    this.confirmingId.set(null);
    this.startEncounter.emit(id);
  }

  /** Snap a vertical drag to the nearest 30-min slot, validate, and emit a reschedule. */
  onDragEnded(event: CdkDragEnd, pe: PositionedEvent): void {
    const slotDelta = Math.round(event.distance.y / this.slotHeight);
    event.source.reset(); // return to origin; a successful move re-renders via reload
    setTimeout(() => (this.wasDragged = false)); // clear after the trailing click is suppressed

    if (slotDelta === 0) return;

    const startMin = minutesFromIso(pe.ref.startTime);
    const duration = minutesFromIso(pe.ref.endTime) - startMin;
    const newStart = startMin + slotDelta * SLOT_MINUTES;

    if (newStart < 0 || newStart + duration > DAY_TOTAL_MINUTES) {
      this.dropBlocked.emit('That time is outside working hours.');
      return;
    }
    if (isMinutesPast(newStart, this.date(), this.now())) {
      this.dropBlocked.emit('Cannot move an appointment to a past time.');
      return;
    }

    const conflict = this.events().some((e) => {
      if (e.id === pe.ref.id && e.type === pe.ref.type) return false; // ignore self
      const s = minutesFromIso(e.startTime);
      const en = minutesFromIso(e.endTime);
      return s < newStart + duration && newStart < en;
    });
    if (conflict) {
      this.dropBlocked.emit('That slot is not available.');
      return;
    }

    this.reschedule.emit({ id: pe.ref.id, start: minutesToLabel(newStart) });
  }

  blockClasses(pe: PositionedEvent): string {
    if (pe.isRest) return 'bg-slate-100 border-slate-300';
    switch (pe.status) {
      case 'InProgress': return 'bg-amber-50 border-amber-400';
      case 'Completed': return 'bg-green-50 border-green-400';
      default: return 'bg-blue-50 border-blue-400';
    }
  }

  statusTextClass(status: string): string {
    switch (status) {
      case 'InProgress': return 'text-amber-600';
      case 'Completed': return 'text-green-600';
      default: return 'text-blue-600';
    }
  }
}
