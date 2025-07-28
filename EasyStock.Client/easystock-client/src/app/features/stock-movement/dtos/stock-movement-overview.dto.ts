import { BaseStockMovementDto } from "./base-stock-movement.dto";

export interface StockMovementOverviewDto extends BaseStockMovementDto {
    productName: string;
}