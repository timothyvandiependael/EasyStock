import { Component, Inject, ViewChild, ElementRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-upload-file-dialog',
  imports: [],
  templateUrl: './upload-file-dialog.html',
  styleUrl: './upload-file-dialog.css'
})
export class UploadFileDialog {
  @ViewChild('fileinput') fileInput!: ElementRef<HTMLInputElement>;

  selectedFile?: File;
  accept: string = '';

  constructor(
    public dialogRef: MatDialogRef<UploadFileDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { accept: string }
  ) {
    this.accept = data.accept || '*/*';
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
  }

  onFileDrop(event: DragEvent) {
    event.preventDefault();
    if (event.dataTransfer?.files.length) {
      this.setSelectedFile(event.dataTransfer.files[0]);
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length) {
      this.setSelectedFile(input.files[0]);
    }
  }

  setSelectedFile(file: File) {
    this.selectedFile = file;
  }

  onUpload() {
    if (!this.selectedFile) return;

    const reader = new FileReader();

    reader.onload = () => {
      const fileString = reader.result as string;
      this.dialogRef.close(fileString);
    };

    // Read as Data URL to get Base64 string for images or binary files
    reader.readAsDataURL(this.selectedFile);
  }

  onCancel() {
    this.dialogRef.close(null);
  }
}
