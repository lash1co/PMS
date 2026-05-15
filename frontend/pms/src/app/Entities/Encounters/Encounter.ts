export interface EncounterInterface {
  encounterId: number;
  patientName: string;
  doctorName: string;
  startTime: string;
  endTime: string;
  encounterReason: string;
  status: string;
}

export interface EncounterSummaryDto {
  id: number;
  appointmentId?: number;
  status: string;
  patientName: string;
  doctorName: string;
  subjective?: string;
  objective?: string;
  assessment?: string;
  plan?: string;
  observationsCount: number;
  conditionsCount: number;
  proceduresCount: number;
  allergiesCount: number;
  prescriptionsCount: number;
  observations: any[]; 
  allergies: any[];
  conditions: any[];
  procedures: any[];
}