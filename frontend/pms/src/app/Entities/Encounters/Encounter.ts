interface EncounterInterface {
  encounterId: number;
  patientName: string;
  startTime: string;
  endTime: string;
  encounterReason: string;
  conditions: conditionInterface[];
  clinicalObservations: clinicalObservationsInterface[];
  clinicalNotes: clinicalNotesInterface[];
  prescriptions: prescriptionInterface[];
}
