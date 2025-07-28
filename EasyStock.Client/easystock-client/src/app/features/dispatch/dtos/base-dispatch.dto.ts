import { CommonDto } from "../../../shared/common.dto";

export interface BaseDispatchDto extends CommonDto {
    id: number;
    comments: string;
    dispatchNumber: string;
    clientId: number;
}