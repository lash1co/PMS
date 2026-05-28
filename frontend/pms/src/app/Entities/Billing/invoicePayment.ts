interface InvoicePaymentInterface {
  id: number;
  amount: number;
  paymentDate: string;
  paymentMethod?: string;
  referenceNumber?: string;
  notes?: string;
}

interface RegisterPaymentInterface {
  amount: number;
  paymentMethod?: string;
  referenceNumber?: string;
  notes?: string;
}
