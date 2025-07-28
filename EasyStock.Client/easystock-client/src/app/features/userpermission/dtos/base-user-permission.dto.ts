import { CommonDto } from "../../../shared/common.dto";

export interface BaseUserPermissionDto extends CommonDto {
    id: number;
    userId: number;
    resource: string;
    canView: boolean;
    canAdd: boolean;
    canEdit: boolean;
    canDelete: boolean;
}