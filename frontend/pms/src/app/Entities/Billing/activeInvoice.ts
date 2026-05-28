interface InvoiceDetailInterface {
  id: number;
  code: string;
  unitPrice: number;
  quantity: number;
  price: number;
  description: string;
}

interface ActiveInvoiceInterface {
  id: number;
  patientName: string;
  amount: number;
  paidAmount: number;
  balance: number;
  status: string;
  issuedDate: string;
  dueDate: string;
  paidDate?: string;
  invoiceDetails: InvoiceDetailInterface[];
  payments: InvoicePaymentInterface[];
}
