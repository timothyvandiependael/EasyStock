import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { ProductService } from '../product-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ProductDetailDto } from '../dtos/product-detail.dto';
import { CreateProductDto } from '../dtos/create-product.dto';
import { UpdateProductDto } from '../dtos/update-product.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-product-edit',
  imports: [EditView],
  templateUrl: './product-edit.html',
  styleUrl: './product-edit.css'
})
export class ProductEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedProduct?: ProductDetailDto;

  @ViewChild(EditView) detailView!: EditView<ProductDetailDto>;


  constructor(
    private productService: ProductService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.productService.getColumns().subscribe({
      next: (columns: ColumnMetaData[]) => {
        this.columnMetaData = columns;

        this.loadRouteParams();
      },
      error: (err) => {
        console.error('Error retrieving columns: ', err);
        this.persistentSnackbar.showError(`Error retrieving fields. Please refresh the page or try again later.`);
      }
    });
  }

  loadRouteParams() {
    this.routeSub = this.route.paramMap.subscribe(params => {
      const mode = params.get('mode') as 'add' | 'edit';
      this.detailMode = mode;

      if (mode === 'edit') {
        const id = Number(params.get('id'));
        this.getByIdSub = this.productService.getById(id).subscribe({
          next: (dto: ProductDetailDto) => {
            this.selectedProduct = dto;
          },
          error: (err) => {
            console.error('Error retrieving product with id ' + id + ': ', err);
            this.persistentSnackbar.showError(`Error retrieving record with id ${id}. If the problem persists, please contact support.`);
          }
        })

      }
    });
  }

  ngOnDestroy() {
    this.routeSub?.unsubscribe();
    this.getColumnsSub?.unsubscribe();
    this.saveAndAddSub?.unsubscribe();
    this.saveNewExitSub?.unsubscribe();
    this.saveExitSub?.unsubscribe();
  }

  handleSaveAndAddAnother(product: CreateProductDto) {
    this.saveAndAddSub = this.productService.add(product).subscribe({
      next: (saved: ProductDetailDto) => {
        this.selectedProduct = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`${saved.name} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving product ', err);
        this.persistentSnackbar.showError(`Error saving ${product.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(product: CreateProductDto) {
    this.saveNewExitSub = this.productService.add(product).subscribe({
      next: (saved: ProductDetailDto) => {
        this.selectedProduct = undefined;
        this.snackBar.open(`${saved.name} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/product']);
      },
      error: (err) => {
        console.error('Error saving product ', err);
        this.persistentSnackbar.showError(`Error saving ${product.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(product: UpdateProductDto) {
    this.saveExitSub = this.productService.edit(product.id, product).subscribe({
      next: () => {
        this.snackBar.open(`${product.name} updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/product']);
      },
      error: (err) => {
        console.error('Error updating product: ', err);
        this.persistentSnackbar.showError(`Error saving ${product.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleCancel() {
    if (this.detailView.form.dirty) {
      this.confirmDialogService.open({
        title: this.detailMode === 'add' ? 'Discard new entry?' : 'Discard changes?',
        message: 'You have unsaved changes. Are you sure you want to cancel?',
        confirmText: 'Yes, discard',
        cancelText: 'Keep editing'
      }).subscribe(cancelled => {
        if (cancelled) this.executeCancel();
      });
    }
    else {
      this.executeCancel();
    }
  }

  executeCancel() {
    this.router.navigate(['app/product']);
  }
}
