import { CreateReceptionDto } from "./create-reception.dto";

export interface UpdateReceptionDto extends CreateReceptionDto {
    id: number;
}