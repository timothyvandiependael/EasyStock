import { CreateSalesOrderDto } from "./create-sales-order.dto";

export interface UpdateSalesOrderDto extends CreateSalesOrderDto {
    id: number;
}