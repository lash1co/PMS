// src/app/pages/encounter-modal/encounter-modal.ts
import { Component, Input, Output, EventEmitter, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { EncounterSummaryDto } from '../../Entities/Encounters/Encounter';
import { EncounterService } from '../../services/encounter/encounter.service';

@Component({
  selector: 'app-encounter-modal',
  standalone: true,
  imports: [CommonModule, MatFormFieldModule, FormsModule],
  templateUrl: './encounter-modal.html',
  styleUrl: './encounter-modal.css',
})
export class EncounterModal implements OnInit {
  // We receive the Encounter ID to fetch its detailed summary
  @Input({ required: true }) encounterId!: number;
  @Output() close = new EventEmitter<void>();
  @Output() completed = new EventEmitter<void>();

  private encounterService = inject(EncounterService);

  // State Signals
  activeTab = signal<'summary' | 'notes' | 'observations' | 'conditions' | 'allergies' | 'procedures'>('summary');
  summary = signal<EncounterSummaryDto | null>(null);
  isLoading = signal<boolean>(true);
  isSaving = signal<boolean>(false);
  errorMessage = signal<string>('');
  successMessage = signal<string>('');

  isEditingNote = signal<boolean>(false);

  // Form Signals
  noteForm = signal({ subjective: '', objective: '', assessment: '', plan: '' });
  observationForm = signal({ category: 'vital-signs', displayName: '', valueString: '', unit: '' });
  conditionForm = signal({ code: '', displayName: '', clinicalStatus: 'Active' });
  allergyForm = signal({ substance: '', criticality: 'Low', reaction: '' });
  procedureForm = signal({ code: '', displayName: '', status: 'Completed' });

  

  // Computed signal to detect if the clinical note is "Complete"
  // Requirement: All 4 fields must have at least 10 characters
  isNoteCompleted = computed(() => {
    const note = this.noteForm();
    const minLength = 10;
    return (
      (note.subjective?.trim().length ?? 0) >= minLength &&
      (note.objective?.trim().length ?? 0) >= minLength &&
      (note.assessment?.trim().length ?? 0) >= minLength &&
      (note.plan?.trim().length ?? 0) >= minLength
    );
  });

  ngOnInit(): void {
    this.loadEncounterData();
  }

  // Fetches data and updates signals
  private loadEncounterData(): void {
    this.isLoading.set(true);
    this.encounterService.getEncounterSummary(this.encounterId).subscribe({
      next: (data) => {
        this.summary.set(data);
        this.noteForm.set({
          subjective: data.subjective || '',
          objective: data.objective || '',
          assessment: data.assessment || '',
          plan: data.plan || ''
        });

        // If the note is already completed, we start in "view mode" (blocked)
        // If it's empty, we allow editing immediately
        this.isEditingNote.set(!this.isNoteCompleted());
        
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load encounter details.');
        this.isLoading.set(false);
      }
    });
  }

  setTab(tab: 'summary' | 'notes' | 'observations' | 'conditions' | 'allergies' | 'procedures'): void {
    this.activeTab.set(tab);
    this.clearMessages();
  }

  private clearMessages(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  toggleEditNote(): void {
    this.isEditingNote.update(val => !val);
    this.clearMessages();
  }

  // --- Actions ---

  saveNotes(): void {
    this.isSaving.set(true);
    this.encounterService.updateClinicalNote(this.encounterId, this.noteForm()).subscribe({
      next: () => {
        this.successMessage.set('Clinical notes updated successfully.');
        this.isSaving.set(false);
        this.isEditingNote.set(false); // Lock fields after saving
        this.loadEncounterData(); 
      },
      error: () => {
        this.errorMessage.set('Failed to save clinical notes.');
        this.isSaving.set(false);
      }
    });
  }

  invalidateEncounter(): void {
  const reason = prompt("Please provide a reason for invalidating this encounter:");
  if (!reason) return;

  this.encounterService.invalidateEncounter(this.encounterId, reason).subscribe({
    next: () => {
      this.successMessage.set("Encounter has been invalidated.");
      this.loadEncounterData(); // Refrescar para ver el nuevo estado
    },
    error: () => this.errorMessage.set("Failed to invalidate encounter.")
  });
}

  addObservation(): void {
    this.encounterService.addObservation(this.encounterId, this.observationForm()).subscribe({
      next: () => {
        this.observationForm.set({ category: 'vital-signs', displayName: '', valueString: '', unit: '' });
        this.loadEncounterData();
      }
    });
  }

  deleteObservation(id: number): void {
    this.encounterService.deleteObservation(this.encounterId, id).subscribe(() => this.loadEncounterData());
  }

  addCondition(): void {
    this.encounterService.addCondition(this.encounterId, this.conditionForm()).subscribe({
      next: () => {
        this.conditionForm.set({ code: '', displayName: '', clinicalStatus: 'Active' });
        this.loadEncounterData();
      }
    });
  }

  deleteCondition(id: number): void {
    this.encounterService.deleteCondition(this.encounterId, id).subscribe(() => this.loadEncounterData());
  }

  addAllergy(): void {
    this.encounterService.addAllergy(this.encounterId, this.allergyForm()).subscribe({
      next: () => {
        this.allergyForm.set({ substance: '', criticality: 'Low', reaction: '' });
        this.loadEncounterData();
      }
    });
  }

  deleteAllergy(id: number): void {
    this.encounterService.deleteAllergy(this.encounterId, id).subscribe(() => this.loadEncounterData());
  }

  addProcedure(): void {
    this.encounterService.addProcedure(this.encounterId, this.procedureForm()).subscribe({
      next: () => {
        this.procedureForm.set({ code: '', displayName: '', status: 'Completed' });
        this.loadEncounterData();
      }
    });
  }

  deleteProcedure(id: number): void {
    this.encounterService.deleteProcedure(this.encounterId, id).subscribe(() => this.loadEncounterData());
  }

  completeEncounter(): void {
    if(confirm('Are you sure you want to complete this encounter? This action locks the record.')) {
      this.isSaving.set(true);
      this.encounterService.completeEncounter(this.encounterId).subscribe({
        next: () => {
          this.completed.emit();
          this.closeModal();
        },
        error: () => {
          this.errorMessage.set('Error completing the encounter.');
          this.isSaving.set(false);
        }
      });
    }
  }

  closeModal(): void {
    this.close.emit();
  }
}