import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { ConfirmDialog, ConfirmDialogData } from './confirm-dialog';

@Injectable({
  providedIn: 'root'
})
export class ConfirmDialogService {
  constructor(private dialog: MatDialog) {}

  open(data: ConfirmDialogData): Observable<boolean> {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      width: '400px',
      data: {
        title: data.title ?? 'Are you sure?',
        message: data.message,
        confirmText: data.confirmText ?? 'Yes',
        cancelText: data.cancelText ?? 'No'
      }
    });

    return dialogRef.afterClosed(); 
  }
}
