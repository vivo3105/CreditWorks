import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../../core/models/api-error';
import { CategoryIconComponent } from '../../../shared/components/category-icon/category-icon.component';
import { Vehicle } from '../../models/vehicle.model';
import { VehicleService } from '../../services/vehicle.service';

export type SortColumn =
  | 'id'
  | 'ownerName'
  | 'manufacturerName'
  | 'yearOfManufacture'
  | 'weight'
  | 'categoryName';

export type SortDirection = 'asc' | 'desc';

// Category is derived from weight, so it sorts by weight (Light < Medium < Heavy)
// rather than alphabetically (Heavy < Light < Medium).
const sortKey: Record<SortColumn, (v: Vehicle) => string | number> = {
  id: (v) => v.id,
  ownerName: (v) => v.ownerName,
  manufacturerName: (v) => v.manufacturerName,
  yearOfManufacture: (v) => v.yearOfManufacture,
  weight: (v) => v.weight,
  categoryName: (v) => v.weight,
};

function compare(a: string | number, b: string | number): number {
  return typeof a === 'string' && typeof b === 'string'
    ? a.localeCompare(b, undefined, { sensitivity: 'base' })
    : (a as number) - (b as number);
}

@Component({
  selector: 'app-vehicle-list',
  imports: [DecimalPipe, RouterLink, CategoryIconComponent],
  templateUrl: './vehicle-list.component.html',
  styleUrl: './vehicle-list.component.scss',
})
export class VehicleListComponent implements OnInit {
  private readonly vehicleService = inject(VehicleService);

  protected readonly vehicles = signal<Vehicle[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  // Errors from row actions (delete) shown above the table without hiding it.
  protected readonly actionError = signal<string | null>(null);
  protected readonly deletingId = signal<number | null>(null);

  protected readonly columns: { key: SortColumn; label: string; numeric?: boolean }[] = [
    { key: 'id', label: 'ID' },
    { key: 'ownerName', label: 'Owner' },
    { key: 'manufacturerName', label: 'Manufacturer' },
    { key: 'yearOfManufacture', label: 'Year' },
    { key: 'weight', label: 'Weight (kg)', numeric: true },
    { key: 'categoryName', label: 'Category' },
  ];

  protected readonly sortColumn = signal<SortColumn>('id');
  protected readonly sortDirection = signal<SortDirection>('asc');

  protected readonly sortedVehicles = computed(() => {
    const key = sortKey[this.sortColumn()];
    const factor = this.sortDirection() === 'asc' ? 1 : -1;

    // Ties fall back to id so the order is stable between clicks.
    return [...this.vehicles()].sort(
      (a, b) => factor * compare(key(a), key(b)) || a.id - b.id,
    );
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.actionError.set(null);

    this.vehicleService.getAll().subscribe({
      next: (vehicles) => {
        this.vehicles.set(vehicles);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load vehicles. Make sure the API is running.');
        this.loading.set(false);
      },
    });
  }

  protected remove(vehicle: Vehicle): void {
    if (!confirm(`Delete vehicle ${vehicle.id} owned by ${vehicle.ownerName}? This cannot be undone.`)) {
      return;
    }

    this.deletingId.set(vehicle.id);
    this.actionError.set(null);

    this.vehicleService.delete(vehicle.id).subscribe({
      next: () => {
        this.vehicles.update((list) => list.filter((v) => v.id !== vehicle.id));
        this.deletingId.set(null);
      },
      error: (error) => {
        this.deletingId.set(null);

        // Already gone (e.g. deleted in another tab): the goal is met, so just drop the row.
        if (error instanceof HttpErrorResponse && error.status === 404) {
          this.vehicles.update((list) => list.filter((v) => v.id !== vehicle.id));
          return;
        }

        this.actionError.set(apiErrorMessage(error, `Could not delete vehicle ${vehicle.id}.`));
      },
    });
  }

  protected sortBy(column: SortColumn): void {
    if (this.sortColumn() === column) {
      this.sortDirection.update((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      this.sortColumn.set(column);
      this.sortDirection.set('asc');
    }
  }

  protected ariaSort(column: SortColumn): 'ascending' | 'descending' | 'none' {
    if (this.sortColumn() !== column) {
      return 'none';
    }

    return this.sortDirection() === 'asc' ? 'ascending' : 'descending';
  }
}
