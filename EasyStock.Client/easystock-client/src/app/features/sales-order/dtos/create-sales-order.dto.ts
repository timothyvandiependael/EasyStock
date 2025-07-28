import { CreateSalesOrderLineDto } from "../../sales-order-line/dtos/create-sales-order-line.dto";

export interface CreateSalesOrderDto {
    comments?: string;
    clientId: number;
    lines: CreateSalesOrderLineDto[];
}