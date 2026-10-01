import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../../core/models/api-error';
import { forkJoin } from 'rxjs';
import { CategoryIcon } from '../../models/category-icon.model';
import { findClash, formatRange } from '../../models/category-rules';
import { Category, SaveCategoryRequest } from '../../models/category.model';
import { CategoryService } from '../../services/category.service';

const weight = Validators.pattern(/^\d+(\.\d{1,2})?$/);

// Add/edit one category, like the vehicle form. The weight range may not clash
// with another category; a clash is shown while typing and blocks saving (the
// API rejects it too).
@Component({
  selector: 'app-category-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './category-form.component.html',
  styleUrl: './category-form.component.scss',
})
export class CategoryFormComponent implements OnInit {
  private readonly categoryService = inject(CategoryService);
  private readonly router = inject(Router);

  // Bound from the :id route parameter; absent when adding.
  readonly id = input<string>();

  protected readonly isEdit = computed(() => this.id() !== undefined);

  protected readonly categories = signal<Category[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saveError = signal<string | null>(null);
  protected readonly submitted = signal(false);

  protected readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)],
    }),
    minWeight: new FormControl<number | null>(null, [Validators.required, Validators.min(0), weight]),
    // Empty means "no upper limit".
    maxWeight: new FormControl<number | null>(null, [Validators.min(0), weight]),
    // One of the configured icon names (chosen from a select).
    icon: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  private readonly formChanges = toSignal(this.form.valueChanges);

  // Problem with the weight range, re-checked on every keystroke; null when fine.
  protected readonly rangeProblem = computed<string | null>(() => {
    this.formChanges();
    const { minWeight, maxWeight } = this.form.getRawValue();

    if (this.loading() || minWeight === null || this.form.controls.minWeight.invalid || this.form.controls.maxWeight.invalid) {
      return null;
    }

    if (maxWeight !== null && maxWeight <= minWeight) {
      return 'Maximum weight must be greater than the minimum weight.';
    }

    const exceptId = this.isEdit() ? Number(this.id()) : null;
    const clash = findClash(this.categories(), { minWeight, maxWeight }, exceptId);

    return clash
      ? `This range (${formatRange(minWeight, maxWeight)}) clashes with '${clash.name}' ` +
          `(${formatRange(clash.minWeight, clash.maxWeight)}). Change this range, or change or delete '${clash.name}' first.`
      : null;
  });

  // Icons configured in the API ("CategoryIcons"); the icon field picks one.
  protected readonly icons = signal<CategoryIcon[]>([]);

  // Set when the category being edited uses an icon that is no longer
  // configured; the user has to pick a new one before saving.
  protected readonly retiredIcon = signal<string | null>(null);

  private readonly iconValue = toSignal(this.form.controls.icon.valueChanges, { initialValue: '' });

  protected readonly selectedIcon = computed(() => {
    const name = this.iconValue();
    return this.icons().find((i) => i.name === name) ?? null;
  });

  ngOnInit(): void {
    forkJoin({ categories: this.categoryService.getAll(), icons: this.categoryService.getIcons() }).subscribe({
      next: ({ categories, icons }) => {
        this.categories.set(categories);
        this.icons.set(icons);

        if (this.isEdit()) {
          this.fillForm(categories);
        }

        this.loading.set(false);
      },
      error: (error) => {
        this.loadError.set(apiErrorMessage(error, 'Could not load categories.'));
        this.loading.set(false);
      },
    });
  }

  protected showError(control: keyof typeof this.form.controls): boolean {
    const c = this.form.controls[control];
    return c.invalid && (c.touched || c.dirty || this.submitted());
  }

  protected save(): void {
    this.submitted.set(true);

    if (this.form.invalid || this.rangeProblem()) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const request: SaveCategoryRequest = {
      name: v.name.trim(),
      minWeight: v.minWeight!,
      maxWeight: v.maxWeight,
      icon: v.icon.trim(),
    };

    const save$ = this.isEdit()
      ? this.categoryService.update(Number(this.id()), request)
      : this.categoryService.create(request);

    this.saving.set(true);
    this.saveError.set(null);

    save$.subscribe({
      next: () => this.router.navigate(['/categories']),
      error: (error) => {
        this.saving.set(false);
        this.saveError.set(
          error instanceof HttpErrorResponse && error.status === 404
            ? 'This category no longer exists. It may have been deleted.'
            : apiErrorMessage(error, 'Could not save the category.'),
        );
      },
    });
  }

  private fillForm(categories: Category[]): void {
    const category = categories.find((c) => c.id === Number(this.id()));

    if (!category) {
      this.loadError.set('Category not found.');
      return;
    }

    const iconConfigured = this.icons().some((i) => i.name === category.icon);

    this.form.setValue({
      name: category.name,
      minWeight: category.minWeight,
      maxWeight: category.maxWeight,
      icon: iconConfigured ? category.icon : '',
    });

    this.retiredIcon.set(iconConfigured ? null : category.icon);
  }
}
