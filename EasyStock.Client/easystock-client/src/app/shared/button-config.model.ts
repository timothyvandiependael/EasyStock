export interface ButtonConfig {
  label: string;
  icon?: string; 
  tooltip?: string;
  color?: 'primary' | 'accent' | 'warn' | 'export' | 'detail' | 'gray';
  disabled?: boolean;
  hidden?: boolean;
  action: string;
}