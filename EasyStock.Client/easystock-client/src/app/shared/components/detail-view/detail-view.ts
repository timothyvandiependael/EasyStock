import { Component, Input, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { FormControl, FormGroup, ValidatorFn } from '@angular/forms';
import { ColumnMetaData } from '../../column-meta-data';
import { ReactiveFormsModule } from '@angular/forms';
import { formatDate } from '@angular/common';
import { Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { LookupDialog } from '../lookup-dialog/lookup-dialog';

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

  constructor(private dialog: MatDialog) { }

  form!: FormGroup;

  lookupDisplayCache: { [lookupField: string]: { [id: string]: string } } = {};

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
      const isSystemField = this.systemFields.includes(col.name);
      debugger;
      if (this.mode === 'add' && isSystemField) continue;

      let initialValue = this.model ? (this.model as any)[col.name] : '';

      if (col.type?.toLowerCase() === 'date') {
        if (initialValue) {
          try {
            const dateObj = new Date(initialValue);
            initialValue = formatDate(dateObj, 'dd/MM/yyyy', 'en-US');
          } catch {
            console.warn(`Invalid date for ${col.name}:`, initialValue);
            initialValue = '';
          }
        }
        else {
          initialValue = "not blocked";
        }

      }

      if (col.name.toLowerCase() == 'bluserid' && !initialValue) {
        initialValue = "not blocked";
      }

      const disabled = !col.isEditable;

      const validators: ValidatorFn[] = [];
      if (col.validationRules) {
        debugger;
        if (col.validationRules.required) {
          validators.push(Validators.required);
        }
        if (col.validationRules.maxLength) {
          validators.push(Validators.maxLength(col.validationRules.maxLength));
        }
        if (col.validationRules.minLength) {
          validators.push(Validators.minLength(col.validationRules.minLength));
        }
        if (col.validationRules.min) {
          validators.push(Validators.min(col.validationRules.min));
        }
        if (col.validationRules.max) {
          validators.push(Validators.max(col.validationRules.max));
        }
        if (col.validationRules.pattern) {
          validators.push(Validators.pattern(col.validationRules.pattern));
        }
        if (col.validationRules.isEmail) {
          validators.push(Validators.email);
        }
        if (col.validationRules.isUrl) {
          validators.push(Validators.pattern(
            /^(https?:\/\/)?([\w-]+(\.[\w-]+)+)([\/\w.-]*)*\/?$/i
          ));
        }
        if (col.validationRules.isPassword) {
          validators.push(Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$/));
        }
      }

      group[col.name] = new FormControl({ value: initialValue, disabled }, validators);

      if (col.isLookup && col.lookupIdField) {
        const idValue = this.model ? (this.model as any)[col.lookupIdField] : null;
        group[col.lookupIdField] = new FormControl(idValue);
      }
    }



    this.form = new FormGroup(group);
  }

  clearForm() {
    if (this.form) {
      this.form.reset();
    }
  }

  get visibleFields(): ColumnMetaData[] {
    return this.metaData.filter(col => {
      const isSystemField = this.systemFields.includes(col.name);
      if (this.mode === 'add' && isSystemField) return false;
      return true;
    });
  }

  onSaveAndAddAnother() {
    this.form.markAllAsTouched()
    if (this.form.valid) this.saveAndAddAnother.emit(this.form.getRawValue());
  }

  onSaveAndExit() {
    this.form.markAllAsTouched()
    if (this.form.valid) this.saveAndExit.emit(this.form.getRawValue());
  }

  onSaveNewAndExit() {
    this.form.markAllAsTouched()
    if (this.form.valid) this.saveNewAndExit.emit(this.form.getRawValue());
  }

  onCancel() {
    this.cancel.emit();
  }

  mapType(type: string): string {
    switch (type) {
      case 'number': return 'number';
      case 'date': return 'text';
      case 'boolean': return 'checkbox';
      default: return 'text';
    }
  }

  getLookupDisplay(col: ColumnMetaData): string {
    const idField = col.lookupIdField!;
    const idValue = this.form.get(idField)?.value;
    if (!idValue) return 'Unassigned';

    return this.lookupDisplayCache[col.lookupIdField!]?.[idValue] ?? '(loading…)';
  }

  openLookup(col: ColumnMetaData) {
    const lookupType = col.lookupTarget;
    if (!lookupType) {
      console.warn('No lookup type configured for column ', col);
      return;
    }

    const dialogRef = this.dialog.open(LookupDialog, {
      width: '500px',
      height: '800px',
      data: { type: lookupType }
    });

    dialogRef.afterClosed().subscribe(result => {
      debugger;
      if (result) {
        console.log('Selected from lookup: ', result);

        if (col.lookupIdField) {
          this.form.get(col.lookupIdField)?.setValue(result.id);

          if (!this.lookupDisplayCache[col.lookupIdField]) {
            this.lookupDisplayCache[col.lookupIdField] = {};
          }
          this.lookupDisplayCache[col.lookupIdField][result.id] = result.name;
        }
        //this.form.get(col.name)?.setValue(result.name);


      }
    })
  }

  isDirty(): boolean {
    return this.form.dirty;
  }
}
