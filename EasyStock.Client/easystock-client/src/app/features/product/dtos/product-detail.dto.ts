import { CategoryOverviewDto } from "../../category/dtos/category-overview.dto";
import { SupplierOverviewDto } from "../../supplier/dtos/supplier-overview.dto";

export interface ProductDetailDto {
    id: number;
    sku: string;
    name: string;
    description?: string;
    photo?: string;
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
    autoRestockSupplier: SupplierOverviewDto;
    suppliers: SupplierOverviewDto[];
    category: CategoryOverviewDto;
}