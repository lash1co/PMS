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

export interface MorbidityReportDto {
    generatedAt: string;
    reportPeriod: string;
    totalDiagnoses: number;
    topConditions: ConditionFrequencyDto[];
}

export interface ConditionFrequencyDto {
    conditionCode: string;
    conditionName: string;
    frequency: number;
    percentage: number;
}

export interface AccountsReceivableReportDto {
    generatedAt: string;
    totalPendingAmount: number;
    totalOverdueAmount: number;
    debtors: DebtorDto[];
}

export interface DebtorDto {
    patientId: number;
    patientName: string;
    phone: string;
    invoiceId: number;
    amountOwed: number;
    dueDate: string;
    isOverdue: boolean;
}

export interface AppointmentDetailDto {
    date: string;
    patientName: string;
    doctorName: string;
    status: string;
    reason: string;
}

export interface AppointmentEfficiencyReportDto {
    generatedAt: string;
    reportPeriod: string;
    totalScheduled: number;
    totalCompleted: number;
    totalCancelled: number;
    completionRate: number;
    previousMonthCompletionRate: number;
    growthPercentage: number;
    appointmentsReference: AppointmentDetailDto[];
}