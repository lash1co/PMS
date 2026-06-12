export interface InvoiceHistoryFilter {
  patientIds: number[] | null;
  startDate: string | null;
  endDate: string | null;
  includeCompleted: boolean;
}

export interface InvoiceHistoryResult {
  invoiceId: number;
  patientName: string;
  invoiceDate: string;
  status: string;
  totalAmount: number;
  paidAmount: number;
  balance: number;
}

export interface InvoiceDetailedView {
  header: InvoiceHeaderSummary;
  patient: PatientInsuranceSummary;
  clinicalContext: ClinicalContextSummary | null;
  items: InvoiceItemDetail[];
  paymentLedger: PaymentTransactionSummary[];
}

export interface InvoiceHeaderSummary {
  invoiceId: number;
  status: string;
  issuedDate: string;
  dueDate: string;
  paidDate: string | null;
  subTotal: number;
  paidAmount: number;
  balance: number;
}

export interface PatientInsuranceSummary {
  patientId: number;
  fullName: string;
  taxId: string | null;
  insuranceProvider: string | null;
  planCategory: string | null;
}

export interface ClinicalContextSummary {
  encounterId: number;
  encounterDate: string;
  doctorName: string | null;
  diagnoses: string[];
  isWalkInCharge: boolean;
}

export interface InvoiceItemDetail {
  detailId: number;
  code: string;
  description: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface PaymentTransactionSummary {
  paymentId: number;
  amount: number;
  paymentDate: string;
  method: string | null;
  reference: string | null;
  notes: string | null;
}