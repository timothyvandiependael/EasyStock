export interface FilterCondition {
  field: string;
  operator: string; // e.g. 'equals', 'contains', etc.
  value: any;
}

export interface SortOption {
  field: string;
  direction: 'asc' | 'desc';
}

export interface Pagination {
  pageIndex: number;
  pageSize: number;
}

export interface AdvancedQueryParametersDto {
  filters?: FilterCondition[];
  sorting?: SortOption[];
  pagination: Pagination;
}

export interface PaginationResult<T> {
  totalCount: number;
  data: T[];
}