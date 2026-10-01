import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { Category } from '../../../categories/models/category.model';
import { CategoryService } from '../../../categories/services/category.service';
import { ManufacturerService } from '../../../manufacturers/services/manufacturer.service';
import { Vehicle } from '../../models/vehicle.model';
import { VehicleService } from '../../services/vehicle.service';
import { VehicleFormComponent } from './vehicle-form.component';

const existing: Vehicle = {
  id: 7, ownerName: 'Vi Vo', manufacturerId: 2, manufacturerName: 'Mercedes',
  yearOfManufacture: 2025, weight: 1800, categoryName: 'Medium', categoryIcon: 'medium_icon',
};

const categories: Category[] = [
  { id: 1, name: 'Light', minWeight: 0, maxWeight: 499.99, icon: 'light_icon' },
  { id: 2, name: 'Medium', minWeight: 500, maxWeight: 2499.99, icon: 'medium_icon' },
  { id: 3, name: 'Heavy', minWeight: 2500, maxWeight: null, icon: 'heavy_icon' },
];

describe('VehicleFormComponent', () => {
  let categories$: Observable<Category[]>;
  let fixture: ComponentFixture<VehicleFormComponent>;
  let vehicleService: { getById: ReturnType<typeof vi.fn>; create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let navigate: ReturnType<typeof vi.spyOn>;

  const el = <T extends HTMLElement>(selector: string) => fixture.nativeElement.querySelector(selector) as T;

  const type = (selector: string, value: string) => {
    const input = el<HTMLInputElement>(selector);
    input.value = value;
    input.dispatchEvent(new Event('input'));
  };

  const setup = async (id?: string, cats: Observable<Category[]> = of(categories)) => {
    categories$ = cats;
    vehicleService = {
      getById: vi.fn(() => of(existing)),
      create: vi.fn(() => of(existing)),
      update: vi.fn(() => of(existing)),
    };

    await TestBed.configureTestingModule({
      imports: [VehicleFormComponent],
      providers: [
        provideRouter([]),
        { provide: VehicleService, useValue: vehicleService },
        { provide: ManufacturerService, useValue: { getAll: () => of([{ id: 1, name: 'Mazda' }, { id: 2, name: 'Mercedes' }]) } },
        { provide: CategoryService, useValue: { getAll: () => categories$, getIcons: () => of([]) } },
      ],
    }).compileComponents();

    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(VehicleFormComponent);
    if (id) {
      fixture.componentRef.setInput('id', id);
    }
    await fixture.whenStable();
  };

  it('creates a vehicle with the entered values and returns to the list', async () => {
    await setup();

    type('#ownerName', '  Jane Doe  ');
    const select = el<HTMLSelectElement>('#manufacturerId');
    select.value = select.options[2].value; // Mercedes
    select.dispatchEvent(new Event('change'));
    type('#yearOfManufacture', '2020');
    type('#weight', '1234.5');

    el<HTMLButtonElement>('button[type="submit"]').click();
    await fixture.whenStable();

    expect(vehicleService.create).toHaveBeenCalledWith({
      ownerName: 'Jane Doe', manufacturerId: 2, yearOfManufacture: 2020, weight: 1234.5,
    });
    expect(navigate).toHaveBeenCalledWith(['/vehicles']);
  });

  it('does not submit an invalid form and shows field errors', async () => {
    await setup();

    type('#weight', '1.234');
    el<HTMLButtonElement>('button[type="submit"]').click();
    await fixture.whenStable();

    expect(vehicleService.create).not.toHaveBeenCalled();
    expect(el('#ownerName-error')).not.toBeNull();
    expect(el('#weight-error').textContent).toContain('at most 2 decimal places');
  });

  it('loads the vehicle for editing and saves with update', async () => {
    await setup('7');

    expect(vehicleService.getById).toHaveBeenCalledWith(7);
    expect(el<HTMLInputElement>('#ownerName').value).toBe('Vi Vo');

    type('#ownerName', 'Vi Vo Updated');
    el<HTMLButtonElement>('button[type="submit"]').click();
    await fixture.whenStable();

    expect(vehicleService.update).toHaveBeenCalledWith(7, {
      ownerName: 'Vi Vo Updated', manufacturerId: 2, yearOfManufacture: 2025, weight: 1800,
    });
  });

  it('shows the API validation message when save is rejected', async () => {
    await setup();
    vehicleService.create.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 400, error: { detail: 'Manufacturer 9 does not exist.' },
    })));

    type('#ownerName', 'Jane');
    const select = el<HTMLSelectElement>('#manufacturerId');
    select.value = select.options[1].value;
    select.dispatchEvent(new Event('change'));
    type('#yearOfManufacture', '2020');
    type('#weight', '100');
    el<HTMLButtonElement>('button[type="submit"]').click();
    await fixture.whenStable();

    expect(el('[role="alert"]').textContent).toContain('Manufacturer 9 does not exist.');
    expect(navigate).not.toHaveBeenCalled();
  });
});

describe('VehicleFormComponent category preview', () => {
  let fixture: ComponentFixture<VehicleFormComponent>;

  const preview = () =>
    (fixture.nativeElement.querySelector('.category-preview') as HTMLElement | null)?.textContent?.trim() ?? null;

  const typeWeight = async (value: string) => {
    const input = fixture.nativeElement.querySelector('#weight') as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();
  };

  const setup = async (categories$: Observable<Category[]>, id?: string) => {
    await TestBed.configureTestingModule({
      imports: [VehicleFormComponent],
      providers: [
        provideRouter([]),
        { provide: VehicleService, useValue: { getById: () => of(existing) } },
        { provide: ManufacturerService, useValue: { getAll: () => of([{ id: 2, name: 'Mercedes' }]) } },
        { provide: CategoryService, useValue: { getAll: () => categories$, getIcons: () => of([]) } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(VehicleFormComponent);
    if (id) {
      fixture.componentRef.setInput('id', id);
    }
    await fixture.whenStable();
  };

  it('updates the category as the weight is typed, with inclusive limits', async () => {
    await setup(of(categories));
    expect(preview()).toContain('Enter a weight to see the category.');

    await typeWeight('499.99');
    expect(preview()).toContain('Light');

    await typeWeight('500');
    expect(preview()).toContain('Medium');

    await typeWeight('3000');
    expect(preview()).toContain('Heavy');

    await typeWeight('1.234'); // invalid: too many decimals
    expect(preview()).toContain('Enter a weight to see the category.');
  });

  it('shows the current category when editing and follows weight changes', async () => {
    await setup(of(categories), '7'); // existing weighs 1800
    expect(preview()).toContain('Medium');

    await typeWeight('100');
    expect(preview()).toContain('Light');
  });

  it('hides the preview but keeps the form usable when categories fail to load', async () => {
    await setup(throwError(() => new Error('down')));

    expect(preview()).toBeNull();
    expect(fixture.nativeElement.querySelector('form')).not.toBeNull();
  });
});
