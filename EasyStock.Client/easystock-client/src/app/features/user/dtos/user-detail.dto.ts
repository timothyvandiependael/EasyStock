import { UserPermissionOverviewDto } from "../../userpermission/dtos/user-permission-overview.dto";
import { BaseUserDto } from "./base-user.dto";

export interface UserDetailDto extends BaseUserDto {
    permissions: UserPermissionOverviewDto[];
}