import { Injectable, inject } from '@angular/core';
import { Observable, catchError, shareReplay, throwError } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { CategoryIcon } from '../models/category-icon.model';
import { Category, SaveCategoryRequest } from '../models/category.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly api = inject(ApiService);

  private icons$?: Observable<CategoryIcon[]>;

  getAll(): Observable<Category[]> {
    return this.api.get<Category[]>('categories');
  }

  create(request: SaveCategoryRequest): Observable<Category> {
    return this.api.post<Category>('categories', request);
  }

  update(id: number, request: SaveCategoryRequest): Observable<Category> {
    return this.api.put<Category>(`categories/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.api.delete(`categories/${id}`);
  }

  // The icon list only changes when the API restarts, so it is fetched once
  // and shared by every page; a failed fetch is retried on the next call.
  getIcons(): Observable<CategoryIcon[]> {
    if (!this.icons$) {
      this.icons$ = this.api.get<CategoryIcon[]>('categories/icons').pipe(
        catchError((error) => {
          this.icons$ = undefined;
          return throwError(() => error);
        }),
        shareReplay(1),
      );
    }

    return this.icons$;
  }
}
