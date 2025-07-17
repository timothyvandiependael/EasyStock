import { CommonDto } from "../../../shared/common.dto";

export interface PurchaseOrderOverviewDto extends CommonDto {
    id: number;
    orderNumber: string;
    comments: string;
    supplierId: number;
    status: string;
    supplierName: string;
}