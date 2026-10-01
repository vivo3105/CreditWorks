import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { Vehicle } from '../../models/vehicle.model';
import { CategoryService } from '../../../categories/services/category.service';
import { VehicleService } from '../../services/vehicle.service';
import { VehicleListComponent } from './vehicle-list.component';

const vehicles: Vehicle[] = [
  { id: 1, ownerName: 'Charlie', manufacturerId: 2, manufacturerName: 'BMW', yearOfManufacture: 2020, weight: 1800, categoryName: 'Medium', categoryIcon: 'medium_icon' },
  { id: 2, ownerName: 'alice', manufacturerId: 15, manufacturerName: 'Toyota', yearOfManufacture: 2024, weight: 3000, categoryName: 'Heavy', categoryIcon: 'heavy_icon' },
  { id: 3, ownerName: 'Bob', manufacturerId: 1, manufacturerName: 'Audi', yearOfManufacture: 2018, weight: 400, categoryName: 'Light', categoryIcon: 'light_icon' },
];

describe('VehicleListComponent sorting', () => {
  let fixture: ComponentFixture<VehicleListComponent>;

  const rowIds = () =>
    Array.from(fixture.nativeElement.querySelectorAll('tbody tr') as NodeListOf<HTMLElement>)
      .map((row) => row.querySelector('td')!.textContent!.trim());

  const header = (label: string) =>
    Array.from(fixture.nativeElement.querySelectorAll('th') as NodeListOf<HTMLElement>)
      .find((th) => th.textContent!.includes(label))!;

  const clickHeader = async (label: string) => {
    header(label).querySelector('button')!.click();
    await fixture.whenStable();
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehicleListComponent],
      providers: [
        provideRouter([]),
        { provide: CategoryService, useValue: { getIcons: () => of([]) } },
        { provide: VehicleService, useValue: { getAll: () => of(vehicles) } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(VehicleListComponent);
    await fixture.whenStable();
  });

  it('sorts by id ascending by default', () => {
    expect(rowIds()).toEqual(['1', '2', '3']);
    expect(header('ID').getAttribute('aria-sort')).toBe('ascending');
  });

  it('sorts text case-insensitively and toggles direction on second click', async () => {
    await clickHeader('Owner');
    expect(rowIds()).toEqual(['2', '3', '1']); // alice, Bob, Charlie

    await clickHeader('Owner');
    expect(rowIds()).toEqual(['1', '3', '2']);
    expect(header('Owner').getAttribute('aria-sort')).toBe('descending');
    expect(header('ID').getAttribute('aria-sort')).toBe('none');
  });

  it('sorts numbers numerically', async () => {
    await clickHeader('Year');
    expect(rowIds()).toEqual(['3', '1', '2']);
  });

  it('sorts category by weight order, not alphabetically', async () => {
    await clickHeader('Category');
    expect(rowIds()).toEqual(['3', '1', '2']); // Light, Medium, Heavy
  });
});

describe('VehicleListComponent actions', () => {
  let fixture: ComponentFixture<VehicleListComponent>;
  let deleteResult: Observable<void>;
  const deleteSpy = vi.fn(() => deleteResult);

  const rowIds = () =>
    Array.from(fixture.nativeElement.querySelectorAll('tbody tr') as NodeListOf<HTMLElement>)
      .map((row) => row.querySelector('td')!.textContent!.trim());

  const deleteButton = (id: number) =>
    fixture.nativeElement.querySelector(`button[aria-label^="Delete vehicle ${id} "]`) as HTMLButtonElement;

  beforeEach(async () => {
    deleteSpy.mockClear();
    deleteResult = of(undefined);

    await TestBed.configureTestingModule({
      imports: [VehicleListComponent],
      providers: [
        provideRouter([]),
        { provide: CategoryService, useValue: { getIcons: () => of([]) } },
        { provide: VehicleService, useValue: { getAll: () => of(vehicles), delete: deleteSpy } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(VehicleListComponent);
    await fixture.whenStable();
  });

  afterEach(() => vi.restoreAllMocks());

  it('links Add Vehicle and the edit icon to the form routes', () => {
    const add = fixture.nativeElement.querySelector('a.btn-primary') as HTMLAnchorElement;
    const edit = fixture.nativeElement.querySelector('a[aria-label^="Edit vehicle 2 "]') as HTMLAnchorElement;

    expect(add.getAttribute('href')).toBe('/vehicles/new');
    expect(edit.getAttribute('href')).toBe('/vehicles/2/edit');
  });

  it('does not delete when the confirmation is cancelled', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    deleteButton(2).click();
    await fixture.whenStable();

    expect(deleteSpy).not.toHaveBeenCalled();
    expect(rowIds()).toEqual(['1', '2', '3']);
  });

  it('deletes and removes the row after confirmation', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    deleteButton(2).click();
    await fixture.whenStable();

    expect(deleteSpy).toHaveBeenCalledWith(2);
    expect(rowIds()).toEqual(['1', '3']);
  });

  it('keeps the row and shows the error when delete fails', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    deleteResult = throwError(() => new HttpErrorResponse({ status: 500 }));

    deleteButton(2).click();
    await fixture.whenStable();

    expect(rowIds()).toEqual(['1', '2', '3']);
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('Could not delete vehicle 2');
  });
});
