import { CommonDto } from "../../../shared/common.dto";

export interface BaseReceptionDto extends CommonDto {
    id: number;
    receptionNumber: string;
    comments?: string;
    supplierId: number;
}