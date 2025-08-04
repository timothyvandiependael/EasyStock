import { AutoRestockDto } from "../../../shared/autorestock.dto";
import { ClientOverviewDto } from "../../client/dtos/client-overview.dto";
import { SalesOrderLineOverviewDto } from "../../sales-order-line/dtos/sales-order-line-overview.dto";
import { BaseSalesOrderDto } from "./base-sales-order.dto";

export interface SalesOrderDetailDto extends BaseSalesOrderDto {
    client: ClientOverviewDto;
    lines: SalesOrderLineOverviewDto[];
    autoRestockDtos: AutoRestockDto[];
}