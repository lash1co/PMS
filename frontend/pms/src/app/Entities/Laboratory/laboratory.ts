interface LaboratoryInterface {
  id: number;
  description: string;
  price: number;
  timeToCompleteInHours: number;
  noFoodBeforeExecuted: boolean;
  liquidIngestionBeforeExecuted: boolean;
}
