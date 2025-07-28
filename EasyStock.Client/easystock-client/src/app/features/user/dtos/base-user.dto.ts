import { CommonDto } from "../../../shared/common.dto";

export interface BaseUserDto extends CommonDto {
    id: number;
    userName: string;
}