import { ProductOverviewDto } from "../../product/dtos/product-overview.dto";
import { PurchaseOrderOverviewDto } from "../../purchase-order/dtos/purchase-order-overview.dto";
import { ReceptionLineOverviewDto } from "../../reception-line/dtos/reception-line-overview.dto";
import { BasePurchaseOrderLineDto } from "./base-purchase-order-line.dto";

export interface PurchaseOrderLineDetailDto extends BasePurchaseOrderLineDto {
    purchaseOrder: PurchaseOrderOverviewDto;
    product: ProductOverviewDto;
    receptionLines: ReceptionLineOverviewDto[];
    deliveredQuantity: number;
}