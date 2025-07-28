import { PersonOverviewDto } from "../../../shared/person/person-overview.dto";
import { SalesOrderOverviewDto } from "../../sales-order/dtos/sales-order-overview.dto";

export interface ClientDetailDto extends PersonOverviewDto {
    salesOrders: SalesOrderOverviewDto[];
}