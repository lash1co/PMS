export interface EncounterInterface {
  encounterId: number;
  patientName: string;
  startTime: string;
  endTime: string;
  encounterReason: string;
}

export interface EncounterSummaryDto {
  encounterId: number;
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