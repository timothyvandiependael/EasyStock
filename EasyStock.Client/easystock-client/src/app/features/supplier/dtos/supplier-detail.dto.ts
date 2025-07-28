import { ProductOverviewDto } from "../../product/dtos/product-overview.dto";
import { PurchaseOrderOverviewDto } from "../../purchase-order/dtos/purchase-order-overview.dto";
import { SupplierOverviewDto } from "./supplier-overview.dto";

export interface SupplierDetailDto extends SupplierOverviewDto {
    purchaseOrders: PurchaseOrderOverviewDto[];
    products: ProductOverviewDto[];
}