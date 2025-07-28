import { UserOverviewDto } from "../../user/dtos/user-overview.dto";
import { BaseUserPermissionDto } from "./base-user-permission.dto";

export interface UserPermissionDetailDto extends BaseUserPermissionDto {
    user: UserOverviewDto;
}