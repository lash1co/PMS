import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EncounterModal } from '../encounter-modal/encounter-modal';
import { EncounterService } from '../../services/encounter/encounter.service';

@Component({
  selector: 'app-encounter.component',
  imports: [CommonModule, FormsModule, EncounterModal],
  templateUrl: './encounter.component.html',
  styleUrl: './encounter.component.css',
})
export class EncounterComponent implements OnInit {
  encountersData = signal<EncounterInterface[]>([]);
  currentEncounter: EncounterInterface = {
    encounterId: 0,
    patientName: '',
    startTime: '',
    endTime: '',
    encounterReason: '',
    conditions: [],
    clinicalObservations: [],
    clinicalNotes: [],
    prescriptions: [],
  };
  showModal = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  constructor(private encounterService: EncounterService) {}

  ngOnInit(): void {
    this.loadEncounters();
  }

  loadEncounters(): void {
    this.encounterService.getEncounters().subscribe({
      next: (encounters) => {
        this.encountersData.set(encounters);
        console.log('Loaded encounters:', encounters);
      },
      error: (error) => {
        console.error('Error loading encounters:', error);
        alert('Failed to load encounters. Please try again.');
      }
    });
  }

  openEncounterModal(encounter: EncounterInterface): void {
    this.currentEncounter = { ...encounter };
    this.showModal.set(true);
  }

  onSave(updatedEncounter: EncounterFormInterface): void {
    // Here you would typically call a service method to save the updated encounter
    console.log('Saving encounter:', updatedEncounter);
    this.showModal.set(false);
    // After saving, you might want to reload the encounters list
    this.loadEncounters();
  }

  closeModal(): void {
    this.showModal.set(false);
  }
}
