import { CreateCategoryDto } from "./create-category.dto";

export interface UpdateCategoryDto extends CreateCategoryDto {
    id: number;
}