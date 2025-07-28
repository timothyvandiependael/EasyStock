import { CreateProductDto } from "./create-product.dto";

export interface UpdateProductDto extends CreateProductDto {
    id: number;
}