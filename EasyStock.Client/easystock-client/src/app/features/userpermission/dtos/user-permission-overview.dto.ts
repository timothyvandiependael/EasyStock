import { BaseUserPermissionDto } from "./base-user-permission.dto";

export interface UserPermissionOverviewDto extends BaseUserPermissionDto {
    userName: string;
}