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
export interface EncounterHistoryFilter {
  startDate: string; // ISO String mapping (YYYY-MM-DD)
  endDate: string;
  patientIds?: number[];
  doctorIds?: number[];
  encounterType?: string;
}

export interface EncounterHistoryResponse {
  encounterId: number;
  patientName: string;
  encounterDate: string;
  encounterType: string;
  reason: string;
  score: number;
}

export interface ClinicalNoteDetail { noteText: string; date: string; }
export interface ClinicalObservationDetail { type: string; value: number; unit: string; date: string; }
export interface ConditionDetail { diagnosis: string; status: string; date: string; }

export interface AppointmentDetail {
  appointmentId: number;
  startTime: string;
  endTime: string;
  reason: string;
  status: string;
}

export interface EncounterHistoryDetail {
  encounterId: number;
  patientName: string;
  doctorName: string;
  startTime: string;
  endTime?: string;
  status: string;
  statusReason?: string;
  updatedBy?: string;
  appointment?: AppointmentDetail;
  notes: ClinicalNoteDetail[];
  vitals: ClinicalObservationDetail[];
  diagnoses: ConditionDetail[];
}