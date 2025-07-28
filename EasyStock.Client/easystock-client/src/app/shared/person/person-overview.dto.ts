import { CommonDto } from "../common.dto";

export interface PersonOverviewDto extends CommonDto {
    id: number;
    name: string;
    address: string;
    city: string;
    postalCode: string;
    country: string;
    phone: string;
    email: string;
    website: string;
}