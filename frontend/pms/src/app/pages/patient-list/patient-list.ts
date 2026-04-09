import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PatientService } from '../../services/patient.service';
import { Patient } from '../../models/patient.model';

@Component({
  selector: 'app-patient-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './patient-list.html'
})
export class PatientList implements OnInit {
  patients: Patient[] = [];
  searchTerm: string = '';

  constructor(private patientService: PatientService) { }

  ngOnInit(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.patientService.getPatients().subscribe({
      next: (data) => this.patients = data,
      error: (err) => console.error('Error loading patients', err)
    });
  }

  onSearch(): void {
    if (this.searchTerm.trim()) {
      this.patientService.searchPatients(this.searchTerm).subscribe({
        next: (data) => this.patients = data,
        error: (err) => console.error('Search failed', err)
      });
    } else {
      this.loadPatients();
    }
  }
}