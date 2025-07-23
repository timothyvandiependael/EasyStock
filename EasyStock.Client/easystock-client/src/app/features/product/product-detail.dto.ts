import { CategoryOverviewDto } from "../category/category-overview.dto";

export interface ProductDetailDto {
    id: number;
    sku: string;
    name: string;
    description?: string;
    // photo: string;
    costPrice: number;
    retailPrice: number;
    discount: number;
    totalStock: number;
    reservedStock: number;
    inboundStock: number;
    availableStock: number;
    backOrderedStock: number;
    minimumStock: number;
    autoRestock: boolean;
    autoRestockSupplierId: number;
    categoryId: number;
    // output auto restock supplier
    // output suppliers
    category: CategoryOverviewDto;
}