import { Medication } from './medication.model';
export interface PrescriptionMedication {
  id: number;

  medicationId: number;   
  medication?: Medication;       
  dosage: string;
  refills: number;

  prescriptionId: number;
}