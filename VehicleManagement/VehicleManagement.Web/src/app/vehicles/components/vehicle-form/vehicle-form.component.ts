import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { findCategoryForWeight } from '../../../categories/models/category-rules';
import { Category } from '../../../categories/models/category.model';
import { CategoryService } from '../../../categories/services/category.service';
import { apiErrorMessage } from '../../../core/models/api-error';
import { CategoryIconComponent } from '../../../shared/components/category-icon/category-icon.component';
import { Manufacturer } from '../../../manufacturers/models/manufacturer.model';
import { ManufacturerService } from '../../../manufacturers/services/manufacturer.service';
import { SaveVehicleRequest } from '../../models/vehicle.model';
import { VehicleService } from '../../services/vehicle.service';

// Rules mirror the Vehicle entity and database column sizes so most mistakes
// are caught before the request; the API still validates everything.
@Component({
  selector: 'app-vehicle-form',
  imports: [ReactiveFormsModule, RouterLink, CategoryIconComponent],
  templateUrl: './vehicle-form.component.html',
  styleUrl: './vehicle-form.component.scss',
})
export class VehicleFormComponent implements OnInit {
  private readonly vehicleService = inject(VehicleService);
  private readonly manufacturerService = inject(ManufacturerService);
  private readonly categoryService = inject(CategoryService);
  private readonly router = inject(Router);

  // Bound from the :id route parameter; absent when adding.
  readonly id = input<string>();

  protected readonly isEdit = computed(() => this.id() !== undefined);

  protected readonly manufacturers = signal<Manufacturer[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saveError = signal<string | null>(null);

  protected readonly form = new FormGroup({
    ownerName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(200), Validators.pattern(/\S/)],
    }),
    manufacturerId: new FormControl<number | null>(null, Validators.required),
    yearOfManufacture: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(1),
      Validators.pattern(/^\d+$/),
    ]),
    weight: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0.01),
      Validators.pattern(/^\d+(\.\d{1,2})?$/),
    ]),
  });

  // The category is never entered: the API works it out from the weight on
  // every read. This preview applies the same rule so it updates as you type.
  // null categories = not loaded (preview hidden; saving still works).
  private readonly categories = signal<Category[] | null>(null);
  private readonly weight = toSignal(this.form.controls.weight.valueChanges, { initialValue: null });

  protected readonly categoryPreview = computed(() => {
    const categories = this.categories();
    if (categories === null) {
      return null;
    }

    const weight = this.weight();
    if (weight === null || this.form.controls.weight.invalid) {
      return { state: 'no-weight' as const };
    }

    const category = findCategoryForWeight(categories, weight);
    return category ? { state: 'match' as const, category } : { state: 'no-match' as const };
  });

  ngOnInit(): void {
    // Only feeds the preview, so a failure here doesn't block the form.
    this.categoryService.getAll().subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => this.categories.set(null),
    });

    this.manufacturerService.getAll().subscribe({
      next: (manufacturers) => {
        this.manufacturers.set(manufacturers);

        if (this.isEdit()) {
          this.loadVehicle(Number(this.id()));
        } else {
          this.loading.set(false);
        }
      },
      error: (error) => this.failLoad(error, 'Could not load manufacturers.'),
    });
  }

  protected showError(control: keyof typeof this.form.controls): boolean {
    const c = this.form.controls[control];
    return c.invalid && (c.touched || c.dirty);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: SaveVehicleRequest = {
      ownerName: value.ownerName.trim(),
      manufacturerId: value.manufacturerId!,
      yearOfManufacture: value.yearOfManufacture!,
      weight: value.weight!,
    };

    const save$ = this.isEdit()
      ? this.vehicleService.update(Number(this.id()), request)
      : this.vehicleService.create(request);

    this.saving.set(true);
    this.saveError.set(null);

    save$.subscribe({
      next: () => this.router.navigate(['/vehicles']),
      error: (error) => {
        this.saving.set(false);
        this.saveError.set(
          error instanceof HttpErrorResponse && error.status === 404
            ? 'This vehicle no longer exists. It may have been deleted.'
            : apiErrorMessage(error, 'Could not save the vehicle.'),
        );
      },
    });
  }

  private loadVehicle(id: number): void {
    this.vehicleService.getById(id).subscribe({
      next: (vehicle) => {
        this.form.setValue({
          ownerName: vehicle.ownerName,
          manufacturerId: vehicle.manufacturerId,
          yearOfManufacture: vehicle.yearOfManufacture,
          weight: vehicle.weight,
        });
        this.loading.set(false);
      },
      error: (error) =>
        this.failLoad(
          error,
          error instanceof HttpErrorResponse && error.status === 404
            ? 'Vehicle not found.'
            : 'Could not load the vehicle.',
        ),
    });
  }

  private failLoad(error: unknown, fallback: string): void {
    this.loadError.set(
      error instanceof HttpErrorResponse && error.status === 0
        ? apiErrorMessage(error, fallback)
        : fallback,
    );
    this.loading.set(false);
  }
}
