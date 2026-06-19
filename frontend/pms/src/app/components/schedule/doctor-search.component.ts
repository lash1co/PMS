import { Component, ChangeDetectionStrategy, input, output, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DoctorOption } from '../../models/doctor.model';

interface SpecialtyGroup {
  specialty: string;
  doctors: DoctorOption[];
}

/**
 * Active doctor search: free-text by name/ID, with specialty quick-filter chips and
 * results grouped by specialty. Filters the preloaded doctor list client-side.
 */
@Component({
  selector: 'app-doctor-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (selected(); as doc) {
      <div class="flex items-center justify-between gap-3 rounded-md border border-blue-200 bg-blue-50 px-3 py-2">
        <span class="text-sm text-blue-900">
          {{ doc.name }}
          @if (doc.specialty) { <span class="ml-1 text-xs text-blue-500">· {{ doc.specialty }}</span> }
        </span>
        <button (click)="cleared.emit()" class="text-xs font-medium text-blue-600 hover:text-blue-800">Change</button>
      </div>
    } @else {
      <div class="relative">
        <input type="text" [(ngModel)]="term" (focus)="open.set(true)"
               placeholder="Search doctor by name or ID…"
               class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500" />

        @if (specialties().length > 1) {
          <div class="mt-2 flex flex-wrap gap-1">
            @for (s of specialties(); track s) {
              <button (click)="toggleSpecialty(s)"
                      class="rounded-full border px-2 py-0.5 text-xs transition-colors"
                      [class.border-blue-500]="specialtyFilter() === s"
                      [class.bg-blue-50]="specialtyFilter() === s"
                      [class.text-blue-700]="specialtyFilter() === s"
                      [class.border-gray-300]="specialtyFilter() !== s"
                      [class.text-gray-600]="specialtyFilter() !== s">
                {{ s }}
              </button>
            }
          </div>
        }

        @if (open() && grouped().length > 0) {
          <div class="absolute z-20 mt-1 max-h-72 w-full overflow-y-auto rounded-md border border-gray-200 bg-white shadow-lg">
            @for (group of grouped(); track group.specialty) {
              <div class="border-b border-gray-100 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-gray-400">
                {{ group.specialty }}
              </div>
              @for (d of group.doctors; track d.id) {
                <button (click)="pick(d)"
                        class="block w-full border-b border-gray-50 px-3 py-2 text-left text-sm last:border-0 hover:bg-blue-50">
                  <strong>{{ d.name }}</strong>
                  <span class="ml-2 text-gray-500">(ID: {{ d.id }})</span>
                </button>
              }
            }
          </div>
        }
      </div>
    }
  `,
})
export class DoctorSearchComponent {
  readonly doctors = input<DoctorOption[]>([]);
  readonly selected = input<DoctorOption | null>(null);

  readonly doctorSelected = output<DoctorOption>();
  readonly cleared = output<void>();

  readonly term = signal('');
  readonly specialtyFilter = signal<string | null>(null);
  readonly open = signal(false);

  readonly specialties = computed(() => {
    const set = new Set<string>();
    for (const d of this.doctors()) if (d.specialty) set.add(d.specialty);
    return [...set].sort();
  });

  private readonly filtered = computed(() => {
    const t = this.term().trim().toLowerCase();
    const sp = this.specialtyFilter();
    return this.doctors().filter(
      (d) => (!sp || d.specialty === sp) && (!t || d.name.toLowerCase().includes(t) || String(d.id).includes(t)),
    );
  });

  readonly grouped = computed<SpecialtyGroup[]>(() => {
    const groups = new Map<string, DoctorOption[]>();
    for (const d of this.filtered()) {
      const key = d.specialty ?? 'Other';
      (groups.get(key) ?? groups.set(key, []).get(key)!).push(d);
    }
    return [...groups.entries()].sort((a, b) => a[0].localeCompare(b[0])).map(([specialty, doctors]) => ({ specialty, doctors }));
  });

  toggleSpecialty(specialty: string): void {
    this.specialtyFilter.set(this.specialtyFilter() === specialty ? null : specialty);
    this.open.set(true);
  }

  pick(doctor: DoctorOption): void {
    this.doctorSelected.emit(doctor);
    this.open.set(false);
    this.term.set('');
    this.specialtyFilter.set(null);
  }
}
