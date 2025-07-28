export interface CreateUserPermissionDto {
    userId: string;
    resource: string;
    canView: boolean;
    canAdd: boolean;
    canEdit: boolean;
    canDelete: boolean;
}