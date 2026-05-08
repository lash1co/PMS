export interface EncounterInterface {
  EncounterId: number;
  patientName: string;
  startTime: string;
  endTime: string;
  encounterReason: string;
  conditions: conditionInterface[];
  clinicalObservations: clinicalObservationsInterface[];
  clinicalNotes: clinicalNotesInterface[];
  prescriptions: prescriptionInterface[];
}

export interface EncounterSummaryDto {
  EncounterId: number;
  patientName: string;
  subjective: string;
  objective: string;
  assessment: string;
  plan: string;
  observationsCount: number;
  conditionsCount: number;
  allergiesCount: number;
  prescriptionsCount: number;
  observations: any[]; 
  allergies: any[];
}