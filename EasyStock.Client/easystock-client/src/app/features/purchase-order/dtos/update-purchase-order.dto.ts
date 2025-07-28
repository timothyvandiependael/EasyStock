import { CreatePurchaseOrderDto } from "./create-purchase-order.dto";

export interface UpdatePurchaseOrderDto extends CreatePurchaseOrderDto {
    id: number;
}