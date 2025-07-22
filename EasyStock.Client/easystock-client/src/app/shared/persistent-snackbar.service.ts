import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarRef, TextOnlySnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class PersistentSnackbarService {
  constructor(private snackBar: MatSnackBar) { }

  showError(message: string): MatSnackBarRef<TextOnlySnackBar> {
    const snackBarRef = this.snackBar.open(message, 'Close', {
      duration: undefined,  // stays until user closes
      panelClass: ['error-snackbar']
    });

    snackBarRef.onAction().subscribe(() => snackBarRef.dismiss());

    return snackBarRef;
  }

  showMessage(message: string, action: string = 'Close', panelClass: string[] = []): MatSnackBarRef<TextOnlySnackBar> {
    const snackBarRef = this.snackBar.open(message, action, {
      duration: undefined,
      panelClass
    });

    snackBarRef.onAction().subscribe(() => snackBarRef.dismiss());

    return snackBarRef;
  }
}
