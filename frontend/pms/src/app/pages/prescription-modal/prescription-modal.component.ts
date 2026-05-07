import { Component, Input, Output, EventEmitter, signal, OnChanges, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PrescriptionService } from '../../services/prescription/prescription.service';
import { Prescription } from '../../models/prescription.model';

import { DoctorService } from '../../services/doctor/doctor.service';
import { PatientService } from '../../services/patient/patient.service';
import { MedicationService } from '../../services/medication/medication.service';
import { PrescriptionMedication } from '../../models/prescription-medication.model';

@Component({
  selector: 'app-prescription-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './prescription-modal.component.html'
})
export class PrescriptionModalComponent implements OnChanges, OnInit {

  @Input() prescription: Prescription | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  doctors = signal<any[]>([]);
  patients = signal<any[]>([]);
  medicationsList = signal<any[]>([]);

  formData = signal<Prescription>({
    id: 0,
    issueDate: '',
    doctorId: 0,
    patientId: 0,
    medications: []
  });

  constructor(
    private service: PrescriptionService,
    private doctorService: DoctorService,
    private patientService: PatientService,
    private medicationService: MedicationService
  ) {}

  ngOnInit() {
    this.doctorService.searchDoctors('').subscribe(r => this.doctors.set(r));
    this.patientService.searchPatients('').subscribe(r => this.patients.set(r));
    this.medicationService.searchMedications('').subscribe(r => {
      this.medicationsList.set(r);
    });
  }

  ngOnChanges() {
    if (this.prescription) {
      this.formData.set({
        ...this.prescription,
        doctorId: Number(this.prescription.doctorId),
        patientId: Number(this.prescription.patientId),
        medications: (this.prescription.medications || []).map(m => ({
          ...m,
          medicationId: Number(m.medicationId)
        }))
      });
    }
  }

  update(field: keyof Prescription, value: any) {
    this.formData.update(f => ({ ...f, [field]: value }));
  }

  addMedication() {
    this.formData.update(f => ({
      ...f,
      medications: [
        ...f.medications,
        {
          id: 0,
          medicationId: 0,
          dosage: '',
          refills: 0,
          prescriptionId: f.id
        } as PrescriptionMedication
      ]
    }));
  }

  updateMed(
  index: number,
  field: keyof PrescriptionMedication,
  value: any
) {
  this.formData.update(f => {
    const meds = [...f.medications];

    (meds[index] as any)[field] = value;

    return {
      ...f,
      medications: meds
    };
  });
}

  removeMedication(index: number) {
    this.formData.update(f => ({
      ...f,
      medications: f.medications.filter((_, i) => i !== index)
    }));
  }

  trackByIndex(index: number, item: PrescriptionMedication) {
    return item.id || index;
  }

  onSave() {
    const data = this.formData();

    if (!data.issueDate) {
      alert('Date is required');
      return;
    }

    if (!data.doctorId) {
      alert('Doctor is required');
      return;
    }

    if (!data.patientId) {
      alert('Patient is required');
      return;
    }

    for (const med of data.medications) {
      if (!med.medicationId) {
        alert('Medication required');
        return;
      }

      if (!med.dosage || med.dosage.trim().length < 2) {
        alert('Dosage inválido');
        return;
      }

      if (med.refills < 0) {
        alert('Refills inválido');
        return;
      }
    }

    const payload = {
      ...data,
      doctor: undefined,
      patient: undefined
    };

    if (data.id) {
      this.service.update(payload).subscribe(() => {
        this.saved.emit();
        this.close.emit();
      });
    } else {
      this.service.create(payload).subscribe(() => {
        this.saved.emit();
        this.close.emit();
      });
    }
  }

  onClose() {
    this.close.emit();
  }
}