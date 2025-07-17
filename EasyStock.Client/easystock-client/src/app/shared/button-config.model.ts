export interface ButtonConfig {
  label: string;
  icon?: string; // Heroicons, Lucide, FontAwesome, or custom
  tooltip?: string;
  color?: 'primary' | 'accent' | 'warn';
  disabled?: boolean;
  hidden?: boolean;
  action: string;
}