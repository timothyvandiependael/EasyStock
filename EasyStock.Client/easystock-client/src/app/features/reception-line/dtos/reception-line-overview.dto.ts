import { BaseReceptionLineDto } from "./base-reception-line.dto";

export interface ReceptionLineOverviewDto extends BaseReceptionLineDto {
    receptionNumber: string;
    productName: string;
}