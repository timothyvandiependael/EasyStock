import { CommonDto } from "../../../shared/common.dto";

export interface BaseSalesOrderLineDto extends CommonDto {
    id: number;
    salesOrderId: number;
    lineNumber: number;
    comments?: string;
    productId: number;
    quantity: number;
    unitPrice: number;
    status: string;
}