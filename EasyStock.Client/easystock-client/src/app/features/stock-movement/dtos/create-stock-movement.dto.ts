export interface CreateStockMovementDto {
    productId: number;
    quantityChange: number;
    reason: string;
    purchaseOrderId?: number;
    salesOrderId?: number;
}