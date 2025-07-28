export interface CreateDispatchLineDto {
    dispatchId: number;
    comments: string;
    productId: number;
    quantity: number;
    salesOrderLineId: number;
}