export interface ValidationRules {
  required?: boolean;
  maxLength: number;
  minLength: number;
  min: number;
  max: number;
  isPassword: boolean;
  isEmail: boolean;
  isUrl: boolean;
  pattern: string;
}

export interface ColumnMetaData {
  name: string;
  type: string;
  isEditable: boolean;
  isFilterable: boolean;
  isSortable: boolean;
  displayName?: string;
  isLookup: boolean;
  lookupIdField?: string;
  isOnlyDetail: boolean;
  validationRules?: ValidationRules;
}