import { BaseSalesOrderDto } from "./base-sales-order.dto";

export interface SalesOrderOverviewDto extends BaseSalesOrderDto {
    clientName: string;
}