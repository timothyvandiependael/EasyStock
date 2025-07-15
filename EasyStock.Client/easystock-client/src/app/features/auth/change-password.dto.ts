export interface ChangePasswordDto {
    userName: string;
    oldPassword?: string;
    newPassword: string;
}