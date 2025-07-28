import { DispatchLineOverviewDto } from "../../dispatch-line/dtos/dispatch-line-overview.dto";
import { ProductOverviewDto } from "../../product/dtos/product-overview.dto";
import { SalesOrderOverviewDto } from "../../sales-order/dtos/sales-order-overview.dto";
import { BaseSalesOrderLineDto } from "./base-sales-order-line.dto";

export interface SalesOrderLineDetailDto extends BaseSalesOrderLineDto {
    salesOrder: SalesOrderOverviewDto;
    product: ProductOverviewDto;
    dispatchLines: DispatchLineOverviewDto[];
    dispatchedQuantity: number;
    decreasedStockBelowMinimum: boolean;
    autoRestocked: boolean;
    autoRestockPurchaseOrderId: number;
    autoRestockPurchaseOrderNumber?: string;
}