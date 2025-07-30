import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StorageService {
  store(key: string, object: any) {
    if (!key || !object) return;

    const json = JSON.stringify(object);
    localStorage.setItem(key, json);
  }

  retrieve(key: string) {
    if (!key) return null;

    var raw = localStorage.getItem(key);
    if (!raw) return null;

    var object = JSON.parse(raw);
    return object;
  }

  delete(key: string) {
    if (!key) return;

    localStorage.removeItem(key);
  }
}
