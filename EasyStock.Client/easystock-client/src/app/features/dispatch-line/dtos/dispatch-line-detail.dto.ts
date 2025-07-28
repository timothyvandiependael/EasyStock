import { CommonDto } from "../../../shared/common.dto";
import { DispatchOverviewDto } from "../../dispatch/dtos/dispatch-overview.dto";
import { ProductOverviewDto } from "../../product/dtos/product-overview.dto";
import { SalesOrderLineOverviewDto } from "../../sales-order-line/dtos/sales-order-line-overview.dto";
import { BaseDispatchLineDto } from "./base-dispatch-line.dto";

export interface DispatchLineDetailDto extends BaseDispatchLineDto {
    dispatch: DispatchOverviewDto;
    product: ProductOverviewDto;
    salesOrderLine: SalesOrderLineOverviewDto;
}