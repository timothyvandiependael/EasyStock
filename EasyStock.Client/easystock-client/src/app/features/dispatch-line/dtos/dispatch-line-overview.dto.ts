import { BaseDispatchLineDto } from "./base-dispatch-line.dto";

export interface DispatchLineOverviewDto extends BaseDispatchLineDto {
    dispatchNumber: string;
    productName: string;
}