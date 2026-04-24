import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PatientService } from '../../services/patient/patient.service';
import { Patient } from '../../models/patient.model';
import { finalize } from 'rxjs/operators';
import { MatButtonModule } from "@angular/material/button";

@Component({
  selector: 'app-patient-list',
  standalone: true,
  imports: [CommonModule, FormsModule, MatButtonModule],
  templateUrl: './patient-list.html'
})
export class PatientList implements OnInit {
  patients = signal<Patient[]>([]);
  showModal = signal<boolean>(false);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  activeTab = signal<'form' | 'summary'>('form');

  generatedInviteUrl = signal<string>('');

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
    this.activeTab.set('form'); 
    this.showModal.set(true);
  }

  openEditModal(patient: Patient): void {
      this.isEditing.set(true);
      this.generatedInviteUrl.set('');
      this.currentPatient = { ...patient };
      if (this.currentPatient.dateOfBirth) {
        this.currentPatient.dateOfBirth = this.currentPatient.dateOfBirth.split('T')[0];
      }
      this.activeTab.set('summary'); 
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
             this.isSaving.set(false);
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

  generateInvite(patientId?: number): void {
    if (!patientId) return;
    this.patientService.generateInviteLink(patientId).subscribe({
      next: (response) => {
        this.generatedInviteUrl.set(response.url);
      },
      error: (err) => alert('Error al generar el enlace.')
    });
  }

  closeInviteModal(): void {
    this.generatedInviteUrl.set('');
  }

  copyToClipboard(): void {
    navigator.clipboard.writeText(this.generatedInviteUrl()).then(() => {
      alert('Link copied to clipboard');
    });
  }

  private getEmptyPatient(): Patient {
    return { firstName: '', lastName: '', dateOfBirth: '', phone: '', email: '' };
  }

  addInsurance(): void {
  if (!this.currentPatient.insurances) {
    this.currentPatient.insurances = [];
  }
    this.currentPatient.insurances.push({
      payerName: '',
      memberId: '',
      planType: '',
      relationshipToSubscriber: 'Self',
      isPrimary: this.currentPatient.insurances.length === 0
    });
  }

  removeInsurance(index: number): void {
    this.currentPatient.insurances?.splice(index, 1);
  }
}