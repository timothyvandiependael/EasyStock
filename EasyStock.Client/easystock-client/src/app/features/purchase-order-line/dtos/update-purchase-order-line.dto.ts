import { CreatePurchaseOrderLineDto } from "./create-purchase-order-line.dto";

export interface UpdatePurchaseOrderLineDto extends CreatePurchaseOrderLineDto {
    id: number;
}