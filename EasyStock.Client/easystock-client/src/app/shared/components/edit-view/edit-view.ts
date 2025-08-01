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
import { StringService } from '../../services/string-service';

@Component({
  selector: 'app-edit-view',
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './edit-view.html',
  styleUrl: './edit-view.css'
})
export class EditView<T> {
  @Input() metaData: ColumnMetaData[] = [];
  @Input() mode: 'add' | 'edit' | 'tabedit' = 'add';
  @Input() model?: T;
  @Input() isProcedureStep1: boolean = false;
  @Input() isProcedureStep2: boolean = false;
  @Input() addModeHideFields: string[] = [];
  @Input() additionalFilters: any;
  @Input() parentType: string = '';
  @Input() filledInFields: any = {};

  @Output() saveAndAddAnother = new EventEmitter<any>();
  @Output() saveAndExit = new EventEmitter<any>();
  @Output() cancel = new EventEmitter<void>();
  @Output() saveNewAndExit = new EventEmitter<any>();
  @Output() save = new EventEmitter<any>();
  @Output() createLines = new EventEmitter<any>();
  @Output() addMoreLines = new EventEmitter<any>();
  @Output() saveAllAndExit = new EventEmitter<any>();
  @Output() procedureCancel = new EventEmitter<any>();


  constructor(private dialog: MatDialog, private stringService: StringService) { }

  form!: FormGroup;

  lookupDisplayCache: { [lookupField: string]: { [id: string]: string } } = {};

  photoUploadColumn?: ColumnMetaData;
  showPhotoUploadPopup = false;

  maxQuantity = 0;

  ngOnInit(): void {
    this.maxQuantity = 0;

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
      const hideInAddMode = this.addModeHideFields.includes(col.name);
      debugger;
      if (this.mode === 'add' && (isSystemField || hideInAddMode)) continue;

      let initialValue = this.model ? (this.model as any)[col.name] : '';
      debugger;
      if (this.filledInFields[col.name])
        initialValue = this.filledInFields[col.name];

      if (this.mode === 'edit' && !initialValue && this.model) {
        var m = this.model as any;
        switch (col.name.toLowerCase()) {
          case "ordernumber":
            if (m.purchaseOrder) {
              initialValue = m.purchaseOrder.orderNumber;
            }
            else if (m.salesOrder)
              initialValue = m.salesOrder.orderNumber;
            break;
          case "receptionnumber":
            if (m.reception) {
              initialValue = m.reception.receptionNumber;
            }
            break;
          case "dispatchnumber":
            if (m.dispatch) {
              initialValue = m.dispatch.dispatchNumber;
            }
            break;
          default:
            break;
        }
      }

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
      const hideInAddMode = this.addModeHideFields.includes(col.name);
      if (this.mode === 'add' && (isSystemField || hideInAddMode)) return false;
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

  onSave() {
    this.form.markAllAsDirty()
    if (this.form.valid) this.save.emit(this.form.getRawValue());
  }

  onCancel() {
    this.cancel.emit();
  }

  onCreateLines() {
    this.form.markAllAsTouched();
    if (this.form.valid) this.createLines.emit(this.form.getRawValue());
  }

  onAddMoreLines() {
    this.form.markAllAsTouched();
    if (this.form.valid) this.addMoreLines.emit(this.form.getRawValue());
  }

  onSaveAllAndExit() {
    this.form.markAllAsTouched();
    if (this.form.valid) this.saveAllAndExit.emit(this.form.getRawValue());
  }

  onProcedureCancel() {
    this.procedureCancel.emit();
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
    var displayValue = this.lookupDisplayCache[col.lookupIdField!]?.[idValue] ?? '';
    if (displayValue == '' && col.lookupTarget != null) {

      if (col.name == "purchaseOrderLink") {
        displayValue = (this.model as any).purchaseOrderLine.purchaseOrder.orderNumber
          + "/"
          + (this.model as any).purchaseOrderLine.lineNumber
      }
      else if (col.name == "salesOrderLink") {
        displayValue = (this.model as any).salesOrderLine.salesOrder.orderNumber
          + "/"
          + (this.model as any).salesOrderLine.lineNumber
      }
      else if (col.name == "orderNumber") {
        displayValue = (this.model as any).purchaseOrder
          ? (this.model as any).purchaseOrder.orderNumber
          : (this.model as any).salesOrder.orderNumber
      }
      else if (col.name == "dispatchNumber") {
        displayValue = (this.model as any).dispatch.dispatchNumber
      }
      else if (col.name == "receptionNumber") {
        displayValue = (this.model as any).reception.receptionNumber
      }
      else {
        var referencedEntity = this.stringService.toLowerFirst(col.lookupTarget);
        var referencedField = this.getReferenceFieldFromLookupFieldName(col.name, referencedEntity);
        displayValue =
          this.model
            ? (this.model as any)[referencedEntity][referencedField]
            : '';
      }
    }

    return displayValue;
  }

  getReferenceFieldFromLookupFieldName(fieldName: string, referencedEntity: string) {
    if (!fieldName || !referencedEntity) return fieldName;
    return this.stringService.toLowerFirst(fieldName.replace(referencedEntity, ''));
  }

  maxQuantityValidator(max: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      return value != null && value > max
        ? { maxQuantity: { requiredMax: max, actual: value } }
        : null;
    };
  }

  openLookup(col: ColumnMetaData) {
    const lookupType = col.lookupTarget;
    if (!lookupType) {
      console.warn('No lookup type configured for column ', col);
      return;
    }

    var filters = this.additionalFilters
      ? (this.additionalFilters[lookupType.toLowerCase()] == undefined
        ? []
        : this.additionalFilters[lookupType.toLowerCase()])
      : [];

    const dialogRef = this.dialog.open(LookupDialog, {
      width: '1000px',
      height: '800px',
      maxWidth: '1000px',
      data: { type: lookupType, filters: filters }
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

          if (col.name == "purchaseOrderLink") {
            this.lookupDisplayCache[col.lookupIdField][result.id] = result.orderNumber + "/" + result.lineNumber;
            this.form.get('productId')?.setValue(result.productId);
            this.form.get('productName')?.setValue(result.productName);
            this.form.get('quantity')?.setValue(result.quantity);
            const quantityControl = this.form.get('quantity');
            this.maxQuantity = result.quantity;
            quantityControl?.setValidators([
              Validators.required,
              this.maxQuantityValidator(result.quantity)
            ])
          }
          else if (col.name == "salesOrderLink") {
            this.lookupDisplayCache[col.lookupIdField][result.id] = result.orderNumber + "/" + result.lineNumber;
            this.form.get('productId')?.setValue(result.productId);
            this.form.get('productName')?.setValue(result.productName);
            this.form.get('quantity')?.setValue(result.quantity);
            this.maxQuantity = result.quantity;
            const quantityControl = this.form.get('quantity');
            quantityControl?.setValidators([
              Validators.required,
              this.maxQuantityValidator(result.quantity)
            ])
          }
          else if (col.name == "orderNumber") {
            this.lookupDisplayCache[col.lookupIdField][result.id] = result.orderNumber;
            if (col.lookupTarget == "SalesOrder") {
              this.form.get('salesOrderId')?.setValue(result.id);
            }
            else {
              this.form.get('purchaseOrderId')?.setValue(result.id);
            }

          }
          else if (col.name == "receptionNumber") {
            this.lookupDisplayCache[col.lookupIdField][result.id] = result.receptionNumber;
            this.form.get('receptionId')?.setValue(result.id);
          }
          else if (col.name == "dispatchNumber") {
            this.lookupDisplayCache[col.lookupIdField][result.id] = result.dispatchNumber;
            this.form.get('dispatchId')?.setValue(result.id);
          }
          else {
            this.lookupDisplayCache[col.lookupIdField][result.id] = result.name;

            if (col.name == 'productName') {
              var price = 0;
              var isSalesOrderLine = this.metaData
              if (this.parentType == 'salesOrder')
                price = result.retailPrice;
              else
                price = result.costPrice;

              this.form.get('unitPrice')?.setValue(price);
            }
          }
        }
      }
    })
  }

  isDirty(): boolean {
    return this.form.dirty;
  }
}
