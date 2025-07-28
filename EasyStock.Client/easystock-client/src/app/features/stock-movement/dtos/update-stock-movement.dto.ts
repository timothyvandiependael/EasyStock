import { CreateStockMovementDto } from "./create-stock-movement.dto";

export interface UpdateStockMovementDto extends CreateStockMovementDto {
    id: number;
}