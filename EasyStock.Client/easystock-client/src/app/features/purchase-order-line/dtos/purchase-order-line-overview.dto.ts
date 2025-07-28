import { BasePurchaseOrderLineDto } from "./base-purchase-order-line.dto";

export interface PurchaseOrderLineOverviewDto extends BasePurchaseOrderLineDto {
    orderNumber: string;
    productName: string;
    deliveredQuantity: number;
}