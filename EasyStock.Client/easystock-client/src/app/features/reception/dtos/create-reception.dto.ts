import { CreateReceptionLineDto } from "../../reception-line/dtos/create-reception-line.dto";

export interface CreateReceptionDto {
    comments?: string;
    supplierId: number;
    lines: CreateReceptionLineDto[];
}