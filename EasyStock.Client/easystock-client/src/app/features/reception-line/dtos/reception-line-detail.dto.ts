import { ProductOverviewDto } from "../../product/dtos/product-overview.dto";
import { PurchaseOrderLineOverviewDto } from "../../purchase-order-line/dtos/purchase-order-line-overview.dto";
import { ReceptionOverviewDto } from "../../reception/dtos/reception-overview.dto";
import { BaseReceptionLineDto } from "./base-reception-line.dto";

export interface ReceptionLineDetailDto extends BaseReceptionLineDto {
    reception: ReceptionOverviewDto;
    product: ProductOverviewDto;
    purchaseOrderLine: PurchaseOrderLineOverviewDto;
}