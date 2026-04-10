import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PatientService } from '../../services/patient.service';
import { Patient } from '../../models/patient.model';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-patient-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './patient-list.html'
})
export class PatientList implements OnInit {
  patients = signal<Patient[]>([]);
  showModal = signal<boolean>(false);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  searchTerm: string = '';
  currentPatient: Patient = this.getEmptyPatient();

  constructor(private patientService: PatientService) { }

  ngOnInit(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.patientService.getPatients().subscribe({
      next: (data) => {
        this.patients.set(data);
      },
      error: (err) => console.error('Error loading patients', err)
    });
  }

  onSearch(): void {
    if (this.searchTerm.trim()) {
      this.patientService.searchPatients(this.searchTerm).subscribe({
        next: (data) => {
          this.patients.set(data);
        },
        error: (err) => console.error('Search failed', err)
      });
    } else {
      this.loadPatients();
    }
  }

  openCreateModal(): void {
    this.isEditing.set(false);
    this.currentPatient = this.getEmptyPatient();
    this.showModal.set(true);
  }

  openEditModal(patient: Patient): void {
    this.isEditing.set(true);
    this.currentPatient = { ...patient };
    
    if (this.currentPatient.dateOfBirth) {
      this.currentPatient.dateOfBirth = this.currentPatient.dateOfBirth.split('T')[0];
    }
    
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.isSaving.set(false); 
  }

  savePatient(): void {
    if (this.isSaving()) return; 

    this.isSaving.set(true);

    if (this.isEditing()) {
      if (!this.currentPatient.id) {
        alert('Error: Cannot update patient without a valid ID.');
        this.isSaving.set(false);
        return; 
      }
      
      this.patientService.updatePatient(this.currentPatient.id, this.currentPatient)
        .pipe(
          finalize(() => {
            this.isSaving.set(false);
          })
        )
        .subscribe({
          next: () => {
            this.loadPatients();
            this.closeModal();
          },
          error: (err) => {
            alert('Update Error: ' + (err.error?.error || 'Please verify the data.'));
          }
        });
    } else {
      this.patientService.createPatient(this.currentPatient)
        .pipe(
          finalize(() => {
             this.isSaving.set(false); // Signal reseting automatically after completion
          })
        ) 
        .subscribe({
          next: () => {
            this.loadPatients();
            this.closeModal();
          },
          error: (err) => {
            console.error('Creation Error:', err); 
            alert('Creation Error: ' + (err.error?.error || 'Please verify the data.'));
          }
        });
    }
  }

  deletePatient(id?: number): void {
    if (id && confirm('Are you sure to remove this patient? This action cannot be undone.')) {
      this.patientService.deletePatient(id).subscribe({
        next: () => this.loadPatients(),
        error: (err) => console.error('Error deleting patient', err)
      });
    }
  }

  private getEmptyPatient(): Patient {
    return { firstName: '', lastName: '', dateOfBirth: '', phone: '', email: '' };
  }
}