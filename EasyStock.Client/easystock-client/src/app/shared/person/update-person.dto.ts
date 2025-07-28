import { CreatePersonDto } from "./create-person.dto";

export interface UpdatePersonDto extends CreatePersonDto {
    id: number;
}