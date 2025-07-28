import { CreateSalesOrderLineDto } from "./create-sales-order-line.dto";

export interface UpdateSalesOrderLineDto extends CreateSalesOrderLineDto {
    id: number;
}