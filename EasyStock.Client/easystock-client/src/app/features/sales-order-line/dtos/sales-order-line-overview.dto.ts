import { BaseSalesOrderLineDto } from "./base-sales-order-line.dto";

export interface SalesOrderLineOverviewDto extends BaseSalesOrderLineDto {
    orderNumber: string;
    productName: string;
    dispatchedQuantity: number;
}