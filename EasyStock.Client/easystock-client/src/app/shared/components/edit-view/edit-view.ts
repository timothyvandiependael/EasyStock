import { Component, Input, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { FormControl, FormGroup, ValidatorFn } from '@angular/forms';
import { ColumnMetaData } from '../../column-meta-data';
import { ReactiveFormsModule } from '@angular/forms';
import { formatDate } from '@angular/common';
import { Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { LookupDialog } from '../lookup-dialog/lookup-dialog';
import { UploadFileDialog } from '../upload-file-dialog/upload-file-dialog';
import { NgClass } from '@angular/common';
import { StringService } from '../../string-service';

@Component({
  selector: 'app-edit-view',
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './edit-view.html',
  styleUrl: './edit-view.css'
})
export class EditView<T> {
  @Input() metaData: ColumnMetaData[] = [];
  @Input() mode: 'add' | 'edit' = 'add';
  @Input() model?: T;

  @Output() saveAndAddAnother = new EventEmitter<any>();
  @Output() saveAndExit = new EventEmitter<any>();
  @Output() cancel = new EventEmitter<void>();
  @Output() saveNewAndExit = new EventEmitter<any>();

  constructor(private dialog: MatDialog, private stringService: StringService) { }

  form!: FormGroup;

  lookupDisplayCache: { [lookupField: string]: { [id: string]: string } } = {};

  photoUploadColumn?: ColumnMetaData;
  showPhotoUploadPopup = false;

  ngOnInit(): void {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['metaData'] || changes['model'] || changes['mode']) {
      this.buildForm();
    }
  }

  requiredLookupValidator(forbiddenValue: string) {
    return (control: AbstractControl) => {
      return control.value === forbiddenValue ? { required: true } : null;
    };
  }

  private systemFields: string[] = ['id', 'crDate', 'crUserId', 'lcDate', 'lcUserId', 'blDate', 'blUserId'];

  private buildForm() {
    const group: Record<string, FormControl> = {};

    for (const col of this.metaData) {
      const isSystemField = this.systemFields.includes(col.name);
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

      // visible field
      group[col.name] = new FormControl({ value: initialValue, disabled }, validators);

      if (col.isLookup && col.lookupIdField) {
        const idValue = this.model ? (this.model as any)[col.lookupIdField] : null;
        // hidden id field
        group[col.lookupIdField] = new FormControl(idValue, validators);
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

  openPhotoUpload(col: ColumnMetaData) {
    this.photoUploadColumn = col;
    this.showPhotoUploadPopup = true;

    const dialogRef = this.dialog.open(UploadFileDialog, {
      data: { accept: 'image/*' },
      width: '400px',
    });

    dialogRef.afterClosed().subscribe((fileString: string | null) => {
      if (fileString) {
        this.form.get(col.name)?.setValue(fileString);
        this.form.get(col.name)?.markAsDirty();
        this.form.get(col.name)?.markAsTouched();
      } else {
        console.log('Upload cancelled');
      }
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
    debugger;
    var displayValue = this.lookupDisplayCache[col.lookupIdField!]?.[idValue] ?? '';
    if (displayValue == '' && col.lookupTarget != null) {
      var referencedEntity = this.stringService.toLowerFirst(col.lookupTarget);
      var referencedField = this.getReferenceFieldFromLookupFieldName(col.name, referencedEntity);
      displayValue = 
        this.model 
        ? (this.model as any)[referencedEntity][referencedField] 
        : '';
    }

    return displayValue;
  }

  getReferenceFieldFromLookupFieldName(fieldName: string, referencedEntity: string) {
    if (!fieldName || !referencedEntity) return fieldName;
    return this.stringService.toLowerFirst(fieldName.replace(referencedEntity, ''));
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
      if (result) {
        if (col.lookupIdField) {
          var ctrl = this.form.get(col.lookupIdField);
          ctrl?.setValue(result.id);
          ctrl?.markAsDirty();

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
