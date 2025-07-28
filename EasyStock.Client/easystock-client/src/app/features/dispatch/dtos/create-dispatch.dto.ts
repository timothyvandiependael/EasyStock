import { CreateDispatchLineDto } from "../../dispatch-line/dtos/create-dispatch-line.dto";

export interface CreateDispatchDto {
    comments: string;
    clientId: number;
    lines: CreateDispatchLineDto[];
}