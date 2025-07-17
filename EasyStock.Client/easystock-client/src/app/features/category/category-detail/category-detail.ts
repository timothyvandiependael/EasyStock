import { Component, OnInit } from '@angular/core';
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

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedCategory?: CategoryOverviewDto;

  constructor(
    private categoryService: CategoryService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar) { }

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
        // TODO: visible error
      }
    });
  }

  loadRouteParams() {
    this.routeSub = this.route.paramMap.subscribe(params => {
      const mode = params.get('mode') as 'add' | 'edit';
      this.detailMode = mode;

      if (mode === 'edit') {
        const id = Number(params.get('id'));

        // TODO fetching category from server by id
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
        this.snackBar.open(`${saved.name} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving category ', err);
        // TODO
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
        // TODO
      }
    })
  }

  handleSaveAndExit(category: UpdateCategoryDto) {
    // TODO
  }

  handleCancel() {
    this.router.navigate(['app/category']);
  }
}
