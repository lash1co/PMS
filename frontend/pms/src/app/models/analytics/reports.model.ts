export interface MonthlyClosingReportDto {
    generatedAt: string;
    reportPeriod: string;
    summary: ClosingSummaryDto;
    doctorMetrics: DoctorProductivityDto[];
}

export interface ClosingSummaryDto {
    totalRevenue: number;
    totalAppointments: number;
    completedAppointments: number;
    cancelledAppointments: number;
    newPatients: number;
}

export interface DoctorProductivityDto {
    doctorId: number;
    doctorName: string;
    appointmentsAttended: number;
    encountersCompleted: number;
    revenueGenerated: number;
}