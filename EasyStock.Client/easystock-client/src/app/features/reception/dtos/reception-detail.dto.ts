import { ReceptionLineOverviewDto } from "../../reception-line/dtos/reception-line-overview.dto";
import { SupplierOverviewDto } from "../../supplier/dtos/supplier-overview.dto";
import { BaseReceptionDto } from "./base-reception.dto";

export interface ReceptionDetailDto extends BaseReceptionDto {
    supplier: SupplierOverviewDto;
    lines: ReceptionLineOverviewDto[];
}