export interface CreatePurchaseOrderLineDto {
    purchaseOrderId: number;
    productId: number;
    comments: string;
    quantity: number;
    unitPrice: number;
}