import { Component, Input, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ColumnMetaData } from '../../column-meta-data';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-detail-view',
  imports: [ReactiveFormsModule],
  templateUrl: './detail-view.html',
  styleUrl: './detail-view.css'
})
export class DetailView<T> {
  @Input() metaData: ColumnMetaData[] = [];
  @Input() mode: 'add' | 'edit' = 'add';
  @Input() model?: T;

  @Output() saveAndAddAnother = new EventEmitter<any>();
  @Output() saveAndExit = new EventEmitter<any>();
  @Output() cancel = new EventEmitter<void>();
  @Output() saveNewAndExit = new EventEmitter<any>();

  form!: FormGroup;

  ngOnInit(): void {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['metaData'] || changes['model'] || changes['mode']) {
      this.buildForm();
    }
  }

  private systemFields: string[] = ['id', 'crDate', 'crUserId', 'lcDate', 'lcUserId', 'blDate', 'blUserId'];

  private buildForm() {
    const group: Record<string, FormControl> = {};

    for (const col of this.metaData) {
      debugger;
      const isSystemField = this.systemFields.includes(col.name);
      if (this.mode === 'add' && isSystemField) continue;

      const initialValue = this.model ? (this.model as any)[col.name] : '';

      const disabled = !col.isEditable;

      group[col.name] = new FormControl({ value: initialValue, disabled });
    }

    this.form = new FormGroup(group);
  }

  get visibleFields(): ColumnMetaData[] {
    return this.metaData.filter(col => {
      const isSystemField = this.systemFields.includes(col.name);
      if (this.mode === 'add' && isSystemField) return false;
      return true;
    });
  }

  onSaveAndAddAnother() {
    if (this.form.valid) this.saveAndAddAnother.emit(this.form.getRawValue());
  }

  onSaveAndExit() {
    if (this.form.valid) this.saveAndExit.emit(this.form.getRawValue());
  }

  onSaveNewAndExit() {
    if (this.form.valid) this.saveNewAndExit.emit(this.form.getRawValue());
  }

  onCancel() {
    this.cancel.emit();
  }

  mapType(type: string): string {
    switch (type) {
      case 'number': return 'number';
      case 'date': return 'date';
      case 'boolean': return 'checkbox';
      default: return 'text';
    }
  }

  getLookupOptions(col: ColumnMetaData) {
    // TODO, REWRITE 
    return [
      { id: '1', label: 'Option 1' },
      { id: '2', label: 'Option 2' },
    ];
  }
}
