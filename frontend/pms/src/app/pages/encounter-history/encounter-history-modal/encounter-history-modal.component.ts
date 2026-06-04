import { Component, input, output, inject, signal, effect } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { EncounterService } from '../../../services/encounter/encounter.service';
import { EncounterHistoryDetail } from '../../../Entities/Encounters/Encounter';

@Component({
  selector: 'app-encounter-history-modal',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './encounter-history-modal.component.html',
  styleUrl: './encounter-history-modal.component.scss'
})
export class EncounterHistoryModalComponent {
  private encounterService = inject(EncounterService);

  encounterId = input.required<number>();
  closeModal = output<void>();

  detail = signal<EncounterHistoryDetail | null>(null);
  isLoading = signal<boolean>(true);
  activeTab = signal<'overview' | 'clinical' | 'appointment'>('overview');

  

  constructor() {
    effect(() => {
      const id = this.encounterId();
      if (id) this.fetchDetails(id);
    });
  }

  private fetchDetails(id: number) {
    this.isLoading.set(true);
    this.encounterService.getEncounterHistoryDetail(id).subscribe({
      next: (data) => {
        this.detail.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching encounter details', err);
        this.isLoading.set(false);
      }
    });
  }
}