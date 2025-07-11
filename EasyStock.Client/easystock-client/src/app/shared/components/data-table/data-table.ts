import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-data-table',
  imports: [],
  templateUrl: './data-table.html',
  styleUrl: './data-table.css'
})
export class DataTable<T extends Record<string, any>> {
  @Input() columns: string[] = [];
  @Input() data: T[] = [];
}
