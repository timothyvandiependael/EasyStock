export interface AutoRestockDto {
    productName: string;
    autoRestocked: boolean;
    autoRestockPurchaseOrderNumber: string;
    productShortage: number;
}