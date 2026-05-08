import { PrescriptionMedication } from './prescription-medication.model';
import { Patient } from './patient.model';

export interface Prescription {
  id: number;
  issueDate: string;

  doctorId: number;
  patientId: number;

  doctor?: {
    id: number;
    name: string;
  };

  patient?: Patient; 

  medications:  PrescriptionMedication[];
  encounterId?: number;
}