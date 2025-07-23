import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { ProductOverviewDto } from './product-overview.dto';
import { CreateProductDto } from './create-product.dto';
import { UpdateProductDto } from './update-product.dto';
import { ProductDetailDto } from './product-detail.dto';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly apiUrl = environment.product;
  
    constructor(private http: HttpClient) {}
  
    getColumns(): Observable<ColumnMetaData[]> {
      return this.http.get<ColumnMetaData[]>(this.apiUrl + 'columns');
    }
  
    getAdvanced(params: AdvancedQueryParametersDto): Observable<PaginationResult<ProductOverviewDto>> {
      if (params.filters) {
        params.filters = params.filters.map(f => ({
          field: f.field,
          operator: f.operator,
          value: f.value != null ? f.value.toString() : ''
        }));
      }
      return this.http.post<PaginationResult<ProductOverviewDto>>(this.apiUrl + 'advanced', params);
    }

    private formatBeforePost(product: CreateProductDto) {
      if (!product.discount) product.discount = 0;
      if (!product.minimumStock) product.minimumStock = 0;
      if (!product.autoRestock) product.autoRestock = false;
    }
  
    add(dto: CreateProductDto): Observable<ProductDetailDto> {
      this.formatBeforePost(dto);
      return this.http.post<ProductDetailDto>(this.apiUrl, dto);
    }
  
    getById(id: number) {
      return this.http.get<ProductDetailDto>(this.apiUrl + 'id/' + id);
    }
  
    edit(id: number, dto: UpdateProductDto) {
      this.formatBeforePost(dto);
      return this.http.put<void>(this.apiUrl + id, dto);
    }
  
    block(id: number) {
      return this.http.post(this.apiUrl + 'block?id=' + id, null);
    }
  
    unblock(id: number) {
      return this.http.post(this.apiUrl + 'unblock?id=' + id, null);
    }
}
