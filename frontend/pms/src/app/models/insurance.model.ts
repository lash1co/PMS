export interface Insurance {
  id?: number;
  payerName: string;
  memberId: string;
  planType?: string;
  relationshipToSubscriber?: string;
  verificationStatus?: string;
  isPrimary?: boolean;
}