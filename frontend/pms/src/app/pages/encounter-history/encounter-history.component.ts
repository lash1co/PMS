import { Component, inject, signal, computed, OnInit, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EncounterService } from '../../services/encounter/encounter.service';
import { PatientService } from '../../services/patient/patient.service';
import { DoctorService } from '../../services/doctor/doctor.service';
import { EncounterHistoryResponse } from '../../Entities/Encounters/Encounter';
import { EncounterHistoryModalComponent } from './encounter-history-modal/encounter-history-modal.component';

@Component({
  selector: 'app-encounter-history',
  standalone: true,
  imports: [CommonModule, FormsModule, EncounterHistoryModalComponent], 
  templateUrl: './encounter-history.component.html'
})
export class EncounterHistoryComponent {
  // Service Injections
  private encounterService = inject(EncounterService);
  private patientService = inject(PatientService);
  private doctorService = inject(DoctorService);

  // Core Filters
  startDate = signal<string>('');
  endDate = signal<string>('');
  encounterType = signal<string>(''); // '', 'With Appointment', 'Emergency'
  
  // Real Database Lists
  dbPatients = signal<{id: number, name: string}[]>([]);
  dbDoctors = signal<{id: number, name: string}[]>([]);

  // Tag / Chips State
  selectedPatientsList = signal<{id: number, name: string}[]>([]);
  selectedDoctorsList = signal<{id: number, name: string}[]>([]);

  // Extracted IDs for API
  selectedPatients = computed(() => this.selectedPatientsList().map(p => p.id));
  selectedDoctors = computed(() => this.selectedDoctorsList().map(d => d.id));

  // Search Input State
  patientSearch = signal<string>('');
  doctorSearch = signal<string>('');

  // Results State
  historyResults = signal<EncounterHistoryResponse[]>([]);
  isLoading = signal<boolean>(false);
  isSearchEnabled = computed(() => !!this.startDate() && !!this.endDate());

  // --- MODAL STATE ---
  selectedEncounterId = signal<number | null>(null);

  constructor() {
    // Al usar afterNextRender, garantizamos que localStorage ya exista
    // porque este código solo se ejecutará del lado del cliente (navegador).
    afterNextRender(() => {
      this.loadCatalogData();
    });
  }
  // --- FETCH REAL DATA ---
  private loadCatalogData() {
    this.patientService.getPatients().subscribe({
      next: (patients) => {
        const mappedPatients = patients.map(p => ({
          id: p.id!,
          name: `${p.lastName} ${p.firstName}`
        }));
        this.dbPatients.set(mappedPatients);
      },
      error: (err) => console.error('Error loading patients catalog', err)
    });

    this.doctorService.getDoctors().subscribe({
      next: (doctors) => {
        const mappedDoctors = doctors.map(d => ({
          id: d.id!,
          name: d.name
        }));
        this.dbDoctors.set(mappedDoctors);
      },
      error: (err) => console.error('Error loading doctors catalog', err)
    });
  }

  // --- AUTOCOMPLETE COMPUTED SIGNALS ---
  filteredPatients = computed(() => {
    const term = this.patientSearch().toLowerCase();
    if (!term) return [];
    return this.dbPatients().filter(p => 
      p.name.toLowerCase().includes(term) && !this.selectedPatients().includes(p.id)
    );
  });

  filteredDoctors = computed(() => {
    const term = this.doctorSearch().toLowerCase();
    if (!term) return [];
    return this.dbDoctors().filter(d => 
      d.name.toLowerCase().includes(term) && !this.selectedDoctors().includes(d.id)
    );
  });

  // --- CHIPS MANAGEMENT ---
  addPatient(patient: {id: number, name: string}) {
    this.selectedPatientsList.update(list => [...list, patient]);
    this.patientSearch.set('');
  }
  removePatient(id: number) {
    this.selectedPatientsList.update(list => list.filter(p => p.id !== id));
  }

  addDoctor(doctor: {id: number, name: string}) {
    this.selectedDoctorsList.update(list => [...list, doctor]);
    this.doctorSearch.set('');
  }
  removeDoctor(id: number) {
    this.selectedDoctorsList.update(list => list.filter(d => d.id !== id));
  }

  // --- MAIN API CALL ---
  searchHistory() {
    if (!this.isSearchEnabled()) return;
    this.isLoading.set(true);

    this.encounterService.getEncounterHistory({
      startDate: this.startDate(),
      endDate: this.endDate(),
      patientIds: this.selectedPatients(),
      doctorIds: this.selectedDoctors(),
      encounterType: this.encounterType()
    }).subscribe({
      next: (results) => {
        this.historyResults.set(results);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching history', err);
        this.isLoading.set(false);
      }
    });
  }

  viewEncounterInfo(encounterId: number) {
    this.selectedEncounterId.set(encounterId);
  }
}