import { ProductOverviewDto } from "../../product/dtos/product-overview.dto";
import { BaseStockMovementDto } from "./base-stock-movement.dto";

export interface StockMovementDetailDto extends BaseStockMovementDto {
    product: ProductOverviewDto;
}