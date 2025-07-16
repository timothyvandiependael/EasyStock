export interface ColumnMetaData {
  name: string;
  type: string;
  isEditable: boolean;
  isFilterable: boolean;
  isSortable: boolean;
  displayName?: string;
  isLookup: boolean;
  lookupIdField?: string;
}