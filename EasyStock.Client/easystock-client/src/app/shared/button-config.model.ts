export interface ButtonConfig {
  label: string;
  icon?: string; 
  tooltip?: string;
  color?: 'primary' | 'accent' | 'warn' | 'export';
  disabled?: boolean;
  hidden?: boolean;
  action: string;
}