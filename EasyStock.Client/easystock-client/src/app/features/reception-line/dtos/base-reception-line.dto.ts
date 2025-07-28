import { CommonDto } from "../../../shared/common.dto";

export interface BaseReceptionLineDto extends CommonDto {
    id: number;
    receptionId: number;
    lineNumber: number;
    comments: string;
    productId: number;
    quantity: number;
}