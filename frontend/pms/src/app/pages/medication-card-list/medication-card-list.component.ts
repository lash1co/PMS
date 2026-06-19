import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Component, Input, Output, OnInit, EventEmitter, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { NumbersOnlyDirective } from '../../Directives/numbers-only.directive';

@Component({
  selector: 'app-medication-card-list',
  imports: [CommonModule, MatCardModule, FormsModule, NumbersOnlyDirective],
  templateUrl: './medication-card-list.component.html',
  styleUrl: './medication-card-list.component.css',
})
export class MedicationCardListComponent implements OnInit {
  @Input({ required: true }) availableMedications!: MedicationInterface[];
  @Output() close = new EventEmitter<void>();
  @Output() add = new EventEmitter<MedicationInterface>();

  isSaving = signal<boolean>(false);
  errorMessage = signal<string>('');

  ngOnInit(): void {
    const initilizedMedications = this.availableMedications.map(med => ({ ...med, dosage: '', refils: 0 }));
    this.availableMedications = initilizedMedications;
  }

  onSave(selectedMedication: MedicationInterface): void {
    if (!this.validateMedication(selectedMedication)) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    this.add.emit(selectedMedication);
    this.isSaving.set(false);
  }

  onClose(): void {
    this.close.emit();
  }

  private validateMedication(selectedMedication: MedicationInterface): boolean {
    if (!selectedMedication.medicationCode || selectedMedication.medicationCode.trim() === '') {
      this.errorMessage.set('Medication code is required.');
      return false;
    }

    if (!selectedMedication.dosage || selectedMedication.dosage.trim() === '') {
      this.errorMessage.set('Dosage is required.');
      return false;
    }

    if (selectedMedication.refils <= 0) {
      this.errorMessage.set('Refills cannot be zero or negative.');
      return false;
    }

    return true;
  }
}
