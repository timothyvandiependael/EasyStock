import { CreateUserPermissionDto } from "./create-user-permission.dto";

export interface UpdateUserPermissionDto extends CreateUserPermissionDto {
    id: number;
}