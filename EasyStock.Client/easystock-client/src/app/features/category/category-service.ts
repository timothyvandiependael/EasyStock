import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { CategoryOverviewDto } from './category-overview.dto';
import { CreateCategoryDto } from './create-category.dto';
import { UpdateCategoryDto } from './update-category.dto';

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

  getById(id: number) {
    return this.http.get<CategoryOverviewDto>(this.apiUrl + 'id/' + id);
  }

  edit(id: number, dto: UpdateCategoryDto) {
    return this.http.put<void>(this.apiUrl + id, dto);
  }

  block(id: number) {
    return this.http.post(this.apiUrl + 'block?id=' + id, null);
  }

  unblock(id: number) {
    return this.http.post(this.apiUrl + 'unblock?id=' + id, null);
  }
}
