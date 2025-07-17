export type FilterOperator = { label: string; value: string };

export const stringOperators: FilterOperator[] = [
  { label: 'Contains', value: 'Contains' },
  { label: 'Equals', value: 'Equals' },
  { label: 'Starts with', value: 'StartsWith' }
];

export const numberOperators: FilterOperator[] = [
  { label: '=', value: 'Equals' },
  { label: '>', value: 'GreaterThan' },
  { label: '≥', value: 'GreaterThanOrEqual' },
  { label: '<', value: 'LessThan' },
  { label: '≤', value: 'LessThanOrEqual' }
];

export const dateOperators: FilterOperator[] = numberOperators; 

export const booleanOperators: FilterOperator[] = [
  { label: 'All', value: 'All' },
  { label: 'True', value: 'True' },
  { label: 'False', value: 'False' },
];