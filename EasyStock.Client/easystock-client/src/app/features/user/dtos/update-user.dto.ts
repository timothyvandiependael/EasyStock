import { CreateUserDto } from "./create-user.dto";

export interface UpdateUserDto extends CreateUserDto {
    id: number;
}