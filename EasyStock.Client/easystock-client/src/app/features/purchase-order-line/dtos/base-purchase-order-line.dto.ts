import { CommonDto } from "../../../shared/common.dto";

export interface BasePurchaseOrderLineDto extends CommonDto {
    id: number;
    purchaseOrderId: number;
    lineNumber: number;
    productId: number;
    comments: string;
    quantity: number;
    unitPrice: number;
    status: string;
}