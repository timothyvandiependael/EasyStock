import { CreatePurchaseOrderLineDto } from "../../purchase-order-line/dtos/create-purchase-order-line.dto";

export interface CreatePurchaseOrderDto {
    comments: string;
    supplierId: number;
    lines: CreatePurchaseOrderLineDto[];
}