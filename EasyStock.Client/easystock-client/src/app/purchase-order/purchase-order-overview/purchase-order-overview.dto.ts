export interface PurchaseOrderOverviewDto {
    id: number;
    orderNumber: string;
    comments: string;
    supplierId: number;
    status: string;
    supplierName: string;
}