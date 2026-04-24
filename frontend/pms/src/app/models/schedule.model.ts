export interface ScheduleView {
  type: string; // 'Appointment' | 'Rest'
  id: number;
  doctorId: number;
  doctorName: string;
  patientId?: number;
  patientName?: string;
  date: string;
  startTime: string;
  endTime: string;
  scheduleStatus: string;
  scheduleDescription: string;
}

export interface ScheduleFilter {
  doctorsId: number[];
  date?: string;
  scheduleStatus?: string;
  patientName?: string;
}

export interface CreateAppointmentDto {
  startTime: string;
  endTime: string;
  status: string;
  reason: string;
  doctor: { id: number };
  patient: { id: number };
}