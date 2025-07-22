import { Component, ViewChild } from '@angular/core';
import { DetailView } from '../../../shared/components/detail-view/detail-view';
import { ActivatedRoute } from '@angular/router';
import { CategoryOverviewDto } from '../category-overview.dto';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { CategoryService } from '../category-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { CreateCategoryDto } from '../create-category.dto';
import { UpdateCategoryDto } from '../update-category.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/persistent-snackbar.service';

@Component({
  selector: 'app-category-detail',
  imports: [DetailView],
  templateUrl: './category-detail.html',
  styleUrl: './category-detail.css'
})
export class CategoryDetail {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedCategory?: CategoryOverviewDto;

  @ViewChild(DetailView) detailView!: DetailView<CategoryOverviewDto>;

  constructor(
    private categoryService: CategoryService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.categoryService.getColumns().subscribe({
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
        this.getByIdSub = this.categoryService.getById(id).subscribe({
          next: (dto: CategoryOverviewDto) => {
            this.selectedCategory = dto;
          },
          error: (err) => {
            console.error('Error retrieving category with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(category: CreateCategoryDto) {
    this.saveAndAddSub = this.categoryService.add(category).subscribe({
      next: (saved: CategoryOverviewDto) => {
        this.selectedCategory = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`${saved.name} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving category ', err);
        this.persistentSnackbar.showError(`Error saving ${category.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(category: CreateCategoryDto) {
    this.saveNewExitSub = this.categoryService.add(category).subscribe({
      next: (saved: CategoryOverviewDto) => {
        this.selectedCategory = undefined;
        this.snackBar.open(`${saved.name} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/category']);
      },
      error: (err) => {
        console.error('Error saving category ', err);
        this.persistentSnackbar.showError(`Error saving ${category.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(category: UpdateCategoryDto) {
    this.saveExitSub = this.categoryService.edit(category.id, category).subscribe({
      next: () => {
        this.snackBar.open(`${category.name} updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/category']);
      },
      error: (err) => {
        console.error('Error updating category: ', err);
        this.persistentSnackbar.showError(`Error saving ${category.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleCancel() {
    this.router.navigate(['app/category']);
  }
}
