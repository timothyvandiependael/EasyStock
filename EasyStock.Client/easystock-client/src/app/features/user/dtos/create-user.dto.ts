import { CreateUserPermissionDto } from "../../userpermission/dtos/create-user-permission.dto";

export interface CreateUserDto {
    userName: string;
    role: string;
    permissions: CreateUserPermissionDto[];
}