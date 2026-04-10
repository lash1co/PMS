import { Component, OnInit, ChangeDetectorRef } from '@angular/core'; 
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
  patients: Patient[] = [];
  searchTerm: string = '';

  showModal: boolean = false;
  isEditing: boolean = false;
  isSaving: boolean = false;
  currentPatient: Patient = this.getEmptyPatient();

  constructor(
    private patientService: PatientService,
    private cdr: ChangeDetectorRef 
  ) { }

  ngOnInit(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.patientService.getPatients().subscribe({
      next: (data) => {
        this.patients = data;
        this.cdr.detectChanges(); 
      },
      error: (err) => console.error('Error loading patients', err)
    });
  }

  onSearch(): void {
    if (this.searchTerm.trim()) {
      this.patientService.searchPatients(this.searchTerm).subscribe({
        next: (data) => {
          this.patients = data;
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Search failed', err)
      });
    } else {
      this.loadPatients();
    }
  }

  openCreateModal(): void {
    this.isEditing = false;
    this.currentPatient = this.getEmptyPatient();
    this.showModal = true;
  }

  openEditModal(patient: Patient): void {
    this.isEditing = true;
    this.currentPatient = { ...patient };
    
    if (this.currentPatient.dateOfBirth) {
      this.currentPatient.dateOfBirth = this.currentPatient.dateOfBirth.split('T')[0];
    }
    
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.isSaving = false;
    this.cdr.detectChanges();
  }

  /// <summary>
  /// Saves the current patient state. Uses RxJS finalize to guarantee the UI unlocks 
  /// regardless of success or unhandled frontend exceptions.
  /// </summary>
savePatient(): void {
    console.log('1. Botón presionado. isSaving actual:', this.isSaving);

    if (this.isSaving) return; 
    this.isSaving = true;

    console.log('2. Iniciando guardado. Datos del paciente:', this.currentPatient);

    if (this.isEditing) {
      if (!this.currentPatient.id) {
        alert('Error: Cannot update patient without a valid ID.');
        this.isSaving = false;
        return; 
      }

      console.log('3. Llamando al servicio UPDATE para el ID:', this.currentPatient.id);
      
      this.patientService.updatePatient(this.currentPatient.id, this.currentPatient)
        .pipe(
          finalize(() => {

            console.log('5. FINALIZE EJECUTADO (El observable terminó, para bien o para mal)');

            this.isSaving = false;
            this.cdr.detectChanges();
          })
        )
        .subscribe({
          next: (response) => {

            console.log('4a. ÉXITO (NEXT). Respuesta del backend:', response);

            this.loadPatients();
            this.closeModal();
          },
          error: (err) => {

            console.error('4b. ERROR (CATCH). El backend devolvió un fallo:', err);

            alert('Update Error: ' + (err.error?.error || 'Please verify the data.'));
          }
        });
    } else {
      // CREATE FLOW
      this.patientService.createPatient(this.currentPatient)
        .pipe(
          finalize(() => {
             this.isSaving = false;
             this.cdr.detectChanges();
          })
        )
        .subscribe({
          next: () => {

            console.log('Paciente creado correctamente, actualizando UI...');

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
        next: () => {
           this.loadPatients();
        },
        error: (err) => console.error('Error deleting patient', err)
      });
    }
  }

  private getEmptyPatient(): Patient {
    return { firstName: '', lastName: '', dateOfBirth: '', phone: '', email: '' };
  }
}