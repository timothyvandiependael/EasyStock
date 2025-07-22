interface ApplyPermissionDto {
    resource: string;
    canAdd: boolean;
    canEdit: boolean;
    canView: boolean;
    canDelete: boolean;
}