import { ClientOverviewDto } from "../../client/dtos/client-overview.dto";
import { DispatchLineOverviewDto } from "../../dispatch-line/dtos/dispatch-line-overview.dto";
import { BaseDispatchDto } from "./base-dispatch.dto";

export interface DispatchDetailDto extends BaseDispatchDto {
    client: ClientOverviewDto;
    lines: DispatchLineOverviewDto[];
}