import { CommonDto } from "../../shared/common.dto";

export interface ProductOverviewDto extends CommonDto {
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
    categoryName: string;
    supplierName: string;
}