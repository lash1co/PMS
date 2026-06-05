import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LaboratoryService } from '../../services/laboratory/laboratory.service';
import { LaboratoryModalComponent } from '../laboratory-modal/laboratory-modal.component';

@Component({
  selector: 'app-laboratory.component',
  imports: [CommonModule, FormsModule, LaboratoryModalComponent],
  templateUrl: './laboratory.component.html',
  styleUrl: './laboratory.component.css',
})
export class LaboratoryComponent implements OnInit {
  laboratoryData = signal<LaboratoryInterface[]>([]);

  currentLaboratory: LaboratoryInterface = {
    id: 0,
    description: '',
    price: 0,
    timeToCompleteInHours: 0,
    noFoodBeforeExecuted: false,
    liquidIngestionBeforeExecuted: false
  };

  showModal = signal<boolean>(false);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  constructor(private laboratoryService: LaboratoryService) {}

  ngOnInit(): void {
    this.loadLaboratories();
  }

  private loadLaboratories(): void {
    this.laboratoryService.getLaboratories().subscribe({
      next: (laboratories) => {
        this.laboratoryData.set(laboratories);
        console.log('Loaded laboratories:', laboratories);
      },
      error: (error) => console.error('Error loading laboratories:', error)
    });
  }

  openCreateModal(): void {
    this.isEditing.set(false);
    this.currentLaboratory = {
      id: 0,
      description: '',
      price: 0,
      timeToCompleteInHours: 0,
      noFoodBeforeExecuted: false,
      liquidIngestionBeforeExecuted: false
    };
    this.showModal.set(true);
  }

  openEditModal(laboratory: LaboratoryInterface): void {
    this.isEditing.set(true);
    this.currentLaboratory = { ...laboratory };
    this.showModal.set(true);
  }

  saveLaboratory(laboratory: LaboratoryInterface): void {
    this.isSaving.set(true);
    if (this.isEditing()) {
      this.laboratoryService.updateLaboratory(laboratory).subscribe({
        next: () => {
          alert(`Laboratory updated successfully!`);
          this.loadLaboratories();
          this.showModal.set(false);
        },
        error: (error) => console.error('Error updating laboratory:', error),
        complete: () => this.isSaving.set(false)
      });
    } else {
      this.laboratoryService.createLaboratory(laboratory).subscribe({
        next: (createdLab) => {
          alert(`Laboratory created successfully!`);
          this.loadLaboratories();
          this.showModal.set(false);
        },
        error: (error) => console.error('Error creating laboratory:', error),
        complete: () => this.isSaving.set(false)
      });
    }
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  deleteLaboratory(id: number): void {
    if (confirm('Are you sure you want to delete this laboratory?')) {
      this.laboratoryService.deleteLaboratory(id).subscribe({
        next: () => {
          this.laboratoryData.update((labs) => labs.filter((lab) => lab.id !== id));
        },
        error: (error) => console.error('Error deleting laboratory:', error)
      });
    }
  }
}
