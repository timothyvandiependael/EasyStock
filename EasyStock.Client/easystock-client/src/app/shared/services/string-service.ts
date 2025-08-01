import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StringService {
  toLowerFirst(text: string) {
    if (!text) return text;
    return text.charAt(0).toLowerCase() + text.slice(1);
  }

  toUpperFirst(text: string) {
    if (!text) return text;
    return text.charAt(0).toUpperCase() + text.slice(1);
  }
}
