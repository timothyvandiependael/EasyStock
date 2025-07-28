import { BasePurchaseOrderDto } from "./base-purchase-order.dto";

export interface PurchaseOrderOverviewDto extends BasePurchaseOrderDto {
    supplierName: string;
}