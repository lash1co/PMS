import { Component, ChangeDetectionStrategy, input, output, computed, linkedSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScheduleView } from '../../models/schedule.model';
import { timeFromIso, addMinutesToLabel } from '../../utils/schedule-time.util';

/**
 * Modal to edit an existing appointment. Patient and reason are read-only (locked);
 * only the time can change. Saving a new time emits `save` (the parent confirms it).
 */
@Component({
  selector: 'app-appointment-edit',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" (click)="cancel.emit()">
      <div class="w-full max-w-md rounded-lg bg-white shadow-xl" (click)="$event.stopPropagation()">
        <div class="flex items-center justify-between border-b border-gray-200 px-5 py-3">
          <h3 class="text-sm font-semibold text-gray-800">Edit appointment</h3>
          <button (click)="cancel.emit()" class="text-gray-400 hover:text-gray-600" aria-label="Close">✕</button>
        </div>

        <div class="flex flex-col gap-4 p-5">
          <div>
            <span class="block text-xs font-medium uppercase tracking-wide text-gray-400">Patient</span>
            <p class="text-sm text-gray-800">{{ appointment().patientName }}</p>
          </div>
          <div>
            <span class="block text-xs font-medium uppercase tracking-wide text-gray-400">Reason</span>
            <p class="text-sm text-gray-800">{{ appointment().scheduleDescription || '—' }}</p>
          </div>

          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Time</label>
            <select [ngModel]="selectedTime()" (ngModelChange)="selectedTime.set($event)"
                    class="block w-full rounded-md border border-gray-300 p-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
              @for (slot of options(); track slot) {
                <option [value]="slot">{{ slot }} – {{ endFor(slot) }}</option>
              }
            </select>
            <p class="mt-1 text-xs text-gray-400">Patient and reason can't be changed here.</p>
          </div>
        </div>

        <div class="flex justify-end gap-2 border-t border-gray-200 px-5 py-3">
          <button (click)="cancel.emit()" class="rounded-md border border-gray-300 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50">Cancel</button>
          <button (click)="onSave()" [disabled]="!changed()"
                  class="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50">
            Save changes
          </button>
        </div>
      </div>
    </div>
  `,
})
export class AppointmentEditComponent {
  readonly appointment = input.required<ScheduleView>();
  readonly availableSlots = input<string[]>([]);

  readonly save = output<{ id: number; start: string }>();
  readonly cancel = output<void>();

  readonly currentStart = computed(() => timeFromIso(this.appointment().startTime));

  /** Free slots plus the appointment's own current time (which otherwise reads as busy). */
  readonly options = computed(() => {
    const set = new Set<string>([this.currentStart(), ...this.availableSlots()]);
    return [...set].sort();
  });

  readonly selectedTime = linkedSignal<string>(() => this.currentStart());
  readonly changed = computed(() => this.selectedTime() !== this.currentStart());

  endFor(slot: string): string {
    return addMinutesToLabel(slot, 30);
  }

  onSave(): void {
    if (!this.changed()) return;
    this.save.emit({ id: this.appointment().id, start: this.selectedTime() });
  }
}
