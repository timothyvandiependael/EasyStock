import { CommonDto } from "../../../shared/common.dto";

export interface BaseUserDto extends CommonDto {
    id: number;
    role: string;
    userName: string;
}