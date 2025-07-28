import { CommonDto } from "../../../shared/common.dto";

export interface BaseStockMovementDto extends CommonDto {
    id: number;
    productId: number;
    quantityChange: number;
    reason: string;
    purchaseOrderId?: number;
    salesOrderId?: number;
}