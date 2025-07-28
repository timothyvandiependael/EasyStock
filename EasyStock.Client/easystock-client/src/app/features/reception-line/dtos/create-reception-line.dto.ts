export interface CreateReceptionLineDto {
    receptionId: number;
    comments: string;
    productId: number;
    quantity: number;
    purchaseOrderLineId: number;
}