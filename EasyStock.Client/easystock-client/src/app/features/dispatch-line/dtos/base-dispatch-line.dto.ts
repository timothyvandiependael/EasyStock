import { CommonDto } from "../../../shared/common.dto";

export interface BaseDispatchLineDto extends CommonDto {
    id: number;
    lineNumber: number;
    dispatchId: number;
    comments: string;
    productId: number;
    quantity: number;
}