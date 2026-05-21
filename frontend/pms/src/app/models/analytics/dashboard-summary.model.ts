export interface DashboardSummary {
  totalAppointments: number;
  totalEncounters: number;
  newPatients: number;
  totalEarnings: number;
  completedAppointments: number;
  upcomingAppointments: number;
  completedEncounters: number;
  topConditions: { conditionName: string, patientCount: number }[];
}