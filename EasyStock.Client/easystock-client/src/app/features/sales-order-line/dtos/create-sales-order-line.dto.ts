export interface CreateSalesOrderLineDto {
    salesOrderId: number;
    comments?: string;
    productId: number;
    quantity: number;
    unitPrice: number;
}