export interface CreateProductDto {
    sku: string;
    name: string;
    description?: string;
    // photo: string;
    costPrice: number;
    retailPrice: number;
    discount: number;
    minimumStock: number;
    autoRestock: boolean;
    autoRestockSupplierId: number;
    categoryId: number;
}