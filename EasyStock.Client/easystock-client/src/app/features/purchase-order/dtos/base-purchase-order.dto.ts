import { CommonDto } from "../../../shared/common.dto";

export interface BasePurchaseOrderDto extends CommonDto {
    id: number;
    orderNumber: string;
    comments: string;
    supplierId: number;
    status: string;
}