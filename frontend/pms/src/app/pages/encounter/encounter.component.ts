import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EncounterModal } from '../encounter-modal/encounter-modal';
import { EncounterService } from '../../services/encounter/encounter.service';
import { PatientService } from '../../services/patient/patient.service';
import { EncounterInterface } from '../../Entities/Encounters/Encounter';
import { Patient } from '../../models/patient.model';
import { DoctorService } from '../../services/doctor/doctor.service';

@Component({
  selector: 'app-encounter',
  standalone: true,
  imports: [CommonModule, FormsModule, EncounterModal],
  templateUrl: './encounter.component.html',
  styleUrl: './encounter.component.css',
})
export class EncounterComponent implements OnInit {

  encountersData = signal<EncounterInterface[]>([]);
  patientsList = signal<Patient[]>([]); 
  doctorsList = signal<any[]>([]);
  
  // Filters
  searchQuery = signal<string>('');
  statusFilter = signal<string>('InProgress'); // Predeterminado
  dateFilter = signal<string>('');

  // Walk-In participants Search
  patientSearch = signal<string>('');
  doctorSearch = signal<string>('');
  
  filteredEncounters = computed(() => {
    let data = this.encountersData();
    const query = this.searchQuery().toLowerCase().trim();
    const status = this.statusFilter();
    const date = this.dateFilter();

    if (status !== 'All') {
      data = data.filter(e => e.status === status);
    }
    if (date) {
      data = data.filter(e => e.startTime.startsWith(date));
    }
    if (query) {
      data = data.filter(e => 
        e.patientName.toLowerCase().includes(query) ||
        e.doctorName.toLowerCase().includes(query) ||
        e.encounterId.toString().includes(query)
      );
    }    
    return data;
  });

  filteredPatients = computed(() => {
    const s = this.patientSearch().toLowerCase();
    return this.patientsList().filter(p => 
      `${p.firstName} ${p.lastName}`.toLowerCase().includes(s)
    ).slice(0, 5);
  });

  filteredDoctors = computed(() => {
    const s = this.doctorSearch().toLowerCase();
    return this.doctorsList().filter(d => 
      d.name.toLowerCase().includes(s)
    ).slice(0, 5);
  });

  selectedEncounterId = signal<number | null>(null);
  showModal = signal<boolean>(false);

  showWalkInModal = signal<boolean>(false);
  isSavingWalkIn = signal<boolean>(false);
  walkInForm = signal({ 
    patientId: null as number | null, 
    doctorId: null as number | null, 
    reason: '' 
  });

  constructor(
    private encounterService: EncounterService,
    private patientService: PatientService,
    private doctorService: DoctorService
  ) {}

  ngOnInit(): void {
    setTimeout(() => {
      this.loadEncounters();
      this.loadPatients();
      this.loadDoctors();
    });
  }

  loadEncounters(): void {
    this.encounterService.getEncounters().subscribe({
      next: (encounters) => this.encountersData.set(encounters || []),
      error: (error: any) => console.error('Error loading encounters:', error)
    });
  }

  loadPatients(): void {
    this.patientService.getPatients().subscribe({
      next: (patients) => this.patientsList.set(patients || []),
      error: (error: any) => console.error('Error loading patients:', error)
    });
  }

  loadDoctors(): void {
    this.doctorService.getDoctors().subscribe({
      next: (docs: any[]) => this.doctorsList.set(docs || []),
      error: (err: any) => console.error('Error loading doctors:', err)
    });
  }

  selectPatient(p: any) {
    this.walkInForm.update(f => ({ ...f, patientId: p.id }));
    this.patientSearch.set(`${p.lastName} ${p.firstName}`);
  }

  selectDoctor(d: any) {
    this.walkInForm.update(f => ({ ...f, doctorId: d.id }));
    this.doctorSearch.set(d.name);
  }

  openWalkInModal(): void {
    this.walkInForm.set({ patientId: null, doctorId: null, reason: '' });
    this.showWalkInModal.set(true);
  }

  closeWalkInModal(): void {
    this.showWalkInModal.set(false);
  }

  submitWalkIn(): void {
    const form = this.walkInForm();
    if (!form.patientId) {
      alert('Please select a patient.');
      return;
    }
    if (!form.doctorId) {
      alert('Please select a doctor.');
      return;
    }

    this.isSavingWalkIn.set(true);
    const request = { PatientId: form.patientId, DoctorId: form.doctorId, InitialReason: form.reason };

    this.encounterService.startWalkIn(request).subscribe({
      next: () => {
        this.isSavingWalkIn.set(false);
        this.closeWalkInModal();
        this.loadEncounters(); 
      },
      error: (err: any) => {
        console.error(err);
        this.isSavingWalkIn.set(false);
        alert('Error creating Walk-In encounter.');
      }
    });
  }

  openEncounterModal(encounter: any): void {
    const id = encounter.encounterId || encounter.EncounterId || encounter.id;
    if (!id) return;
    this.selectedEncounterId.set(id);
    this.showModal.set(true);
  }

  onEncounterCompleted(): void {
    this.showModal.set(false);
    this.selectedEncounterId.set(null);
    this.loadEncounters();
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedEncounterId.set(null);
  }
}