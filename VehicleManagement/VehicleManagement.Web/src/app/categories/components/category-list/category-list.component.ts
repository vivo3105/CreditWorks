import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { CategoryIconComponent } from '../../../shared/components/category-icon/category-icon.component';
import { apiErrorMessage } from '../../../core/models/api-error';
import { findGaps, formatRange } from '../../models/category-rules';
import { Category } from '../../models/category.model';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-category-list',
  imports: [DecimalPipe, RouterLink, CategoryIconComponent],
  templateUrl: './category-list.component.html',
  styleUrl: './category-list.component.scss',
})
export class CategoryListComponent implements OnInit {
  private readonly categoryService = inject(CategoryService);

  protected readonly categories = signal<Category[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  // Errors from row actions (delete) shown above the table without hiding it.
  protected readonly actionError = signal<string | null>(null);
  protected readonly deletingId = signal<number | null>(null);

  // Configured icon names; null until loaded (or if loading fails), so the
  // "not in icon list" note never shows on a guess.
  private readonly iconNames = toSignal(
    this.categoryService.getIcons().pipe(
      map((icons) => new Set(icons.map((i) => i.name))),
      catchError(() => of(null)),
    ),
    { initialValue: null },
  );

  protected readonly iconsLoaded = computed(() => this.iconNames() !== null);

  protected isConfiguredIcon(name: string): boolean {
    return this.iconNames()?.has(name) ?? true;
  }

  // Weight ranges no category covers; vehicles there are uncategorised.
  protected readonly gaps = computed(() =>
    findGaps(this.categories()).map((g) => formatRange(g.minWeight, g.maxWeight)),
  );

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.actionError.set(null);

    // The API returns categories ordered by MinWeight (lightest first).
    this.categoryService.getAll().subscribe({
      next: (categories) => {
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error, 'Could not load categories.'));
        this.loading.set(false);
      },
    });
  }

  protected remove(category: Category): void {
    this.actionError.set(null);

    const range = formatRange(category.minWeight, category.maxWeight);
    if (!confirm(`Delete category '${category.name}' (${range})?\n\nVehicles weighing ${range} will be uncategorised.`)) {
      return;
    }

    this.deletingId.set(category.id);

    this.categoryService.delete(category.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.refreshAfterChange();
      },
      error: (error) => {
        this.deletingId.set(null);

        // Already gone (e.g. deleted in another tab): just show the current list.
        if (error instanceof HttpErrorResponse && error.status === 404) {
          this.refreshAfterChange();
          return;
        }

        this.actionError.set(apiErrorMessage(error, `Could not delete category '${category.name}'.`));
      },
    });
  }

  // Re-read the list quietly (no loading state, so the table doesn't flash).
  private refreshAfterChange(): void {
    this.categoryService.getAll().subscribe({
      next: (categories) => this.categories.set(categories),
      error: (error) => this.actionError.set(apiErrorMessage(error, 'Could not reload categories.')),
    });
  }
}
