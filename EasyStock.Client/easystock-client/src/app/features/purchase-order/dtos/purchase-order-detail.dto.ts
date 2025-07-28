import { PurchaseOrderLineOverviewDto } from "../../purchase-order-line/dtos/purchase-order-line-overview.dto";
import { SupplierOverviewDto } from "../../supplier/dtos/supplier-overview.dto";
import { BasePurchaseOrderDto } from "./base-purchase-order.dto";

export interface PurchaseOrderDetailDto extends BasePurchaseOrderDto {
    supplier: SupplierOverviewDto;
    lines: PurchaseOrderLineOverviewDto[];
}