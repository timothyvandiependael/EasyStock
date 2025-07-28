import { CommonDto } from "../../../shared/common.dto";

export interface BaseSalesOrderDto extends CommonDto {
    id: number;
    orderNumber: string;
    comments?: string;
    clientId: number;
    status: string;
}