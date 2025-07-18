import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { CategoryOverviewDto } from './category-overview.dto';
import { CreateCategoryDto } from './create-category.dto';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly apiUrl = environment.category;

  constructor(private http: HttpClient) {}

  getColumns(): Observable<ColumnMetaData[]> {
    return this.http.get<ColumnMetaData[]>(this.apiUrl + 'columns');
  }

  getAdvanced(params: AdvancedQueryParametersDto): Observable<PaginationResult<CategoryOverviewDto>> {
    if (params.filters) {
      params.filters = params.filters.map(f => ({
        field: f.field,
        operator: f.operator,
        value: f.value != null ? f.value.toString() : ''
      }));
    }
    return this.http.post<PaginationResult<CategoryOverviewDto>>(this.apiUrl + 'advanced', params);
  }

  add(dto: CreateCategoryDto): Observable<CategoryOverviewDto> {
    return this.http.post<CategoryOverviewDto>(this.apiUrl, dto);
  }


}
