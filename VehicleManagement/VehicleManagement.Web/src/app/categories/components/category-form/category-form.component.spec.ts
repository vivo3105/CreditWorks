import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Category } from '../../models/category.model';
import { CategoryService } from '../../services/category.service';
import { CategoryFormComponent } from './category-form.component';

const categories: Category[] = [
  { id: 1, name: 'Light', minWeight: 0, maxWeight: 499.99, icon: 'light_icon' },
  { id: 2, name: 'Medium', minWeight: 500, maxWeight: 2499.99, icon: 'medium_icon' },
  { id: 3, name: 'Heavy', minWeight: 2500, maxWeight: 4999.99, icon: 'heavy_icon' }, // 5000+ is a gap
];

const icons = [
  { name: 'light_icon', imageUrl: 'icons/light.svg' },
  { name: 'medium_icon', imageUrl: 'icons/medium.svg' },
  { name: 'heavy_icon', imageUrl: 'icons/heavy.svg' },
  { name: 'very_heavy_icon', imageUrl: 'icons/very-heavy.svg' },
];

describe('CategoryFormComponent', () => {
  let fixture: ComponentFixture<CategoryFormComponent>;
  let service: {
    getAll: ReturnType<typeof vi.fn>;
    getIcons: ReturnType<typeof vi.fn>;
    create: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
  };
  let navigate: ReturnType<typeof vi.spyOn>;

  const el = <T extends HTMLElement>(selector: string) => fixture.nativeElement.querySelector(selector) as T;
  const type = async (selector: string, value: string) => {
    const input = el<HTMLInputElement>(selector);
    input.value = value;
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();
  };
  const pickIcon = async (name: string) => {
    const select = el<HTMLSelectElement>('#icon');
    select.value = name;
    select.dispatchEvent(new Event('change'));
    await fixture.whenStable();
  };
  const submit = async () => {
    el<HTMLButtonElement>('button[type="submit"]').click();
    await fixture.whenStable();
  };

  const setup = async (id?: string, cats: Category[] = categories) => {
    service = {
      getAll: vi.fn(() => of(cats)),
      getIcons: vi.fn(() => of(icons)),
      create: vi.fn((r) => of({ id: 4, ...r })),
      update: vi.fn((id, r) => of({ id, ...r })),
    };

    await TestBed.configureTestingModule({
      imports: [CategoryFormComponent],
      providers: [provideRouter([]), { provide: CategoryService, useValue: service }],
    }).compileComponents();

    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(CategoryFormComponent);
    if (id) {
      fixture.componentRef.setInput('id', id);
    }
    await fixture.whenStable();
  };

  it('adds a category into a gap and sends only that category', async () => {
    await setup();

    await type('#name', 'Very Heavy');
    await type('#minWeight', '5000');
    await pickIcon('very_heavy_icon');

    expect(el('.range-error')).toBeNull();
    await submit();

    expect(service.create).toHaveBeenCalledWith({
      name: 'Very Heavy', minWeight: 5000, maxWeight: null, icon: 'very_heavy_icon',
    });
    expect(navigate).toHaveBeenCalledWith(['/categories']);
  });

  it('shows a clash while typing and does not save', async () => {
    await setup();

    await type('#name', 'Middle');
    await type('#minWeight', '1000');
    await type('#maxWeight', '2000');
    await pickIcon('medium_icon');

    expect(el('.range-error').textContent).toContain("clashes with 'Medium' (500 – 2,499.99 kg)");

    await submit();
    expect(service.create).not.toHaveBeenCalled();
  });

  it("blocks a minimum equal to another category's maximum", async () => {
    await setup('2');

    await type('#minWeight', '499.99');

    expect(el('.range-error').textContent).toContain("clashes with 'Light' (0 – 499.99 kg)");
    await submit();
    expect(service.update).not.toHaveBeenCalled();
  });

  it('reports max not above min', async () => {
    await setup();

    await type('#minWeight', '6000');
    await type('#maxWeight', '6000');

    expect(el('.range-error').textContent).toContain('greater than the minimum');
  });

  it('edits a category without clashing with its own old range', async () => {
    await setup('2');

    expect(el<HTMLInputElement>('#name').value).toBe('Medium');

    await type('#minWeight', '600');
    await type('#maxWeight', '2400');
    expect(el('.range-error')).toBeNull();

    await submit();
    expect(service.update).toHaveBeenCalledWith(2, {
      name: 'Medium', minWeight: 600, maxWeight: 2400, icon: 'medium_icon',
    });
  });

  it('blocks an edit that would clash with a neighbour', async () => {
    await setup('2');

    await type('#maxWeight', '3000');

    expect(el('.range-error').textContent).toContain("clashes with 'Heavy'");
    await submit();
    expect(service.update).not.toHaveBeenCalled();
  });

  it('offers the configured icons in a select and previews the chosen image', async () => {
    await setup();

    const options = Array.from(el<HTMLSelectElement>('#icon').options).map((o) => o.textContent!.trim());
    expect(options).toEqual(['Select an icon', 'light_icon', 'medium_icon', 'heavy_icon', 'very_heavy_icon']);
    expect(el('.icon-preview img')).toBeNull();

    await pickIcon('heavy_icon');
    expect(el('.icon-preview img').getAttribute('src')).toBe('icons/heavy.svg');
  });

  it('shows the current icon when editing', async () => {
    await setup('3');

    expect(el<HTMLSelectElement>('#icon').value).toBe('heavy_icon');
    expect(el('.icon-preview img').getAttribute('src')).toBe('icons/heavy.svg');
  });

  it('makes the user pick a new icon when the current one is no longer configured', async () => {
    await setup('2', [categories[0], { ...categories[1], icon: 'aaaa' }, categories[2]]);

    expect(el<HTMLSelectElement>('#icon').value).toBe('');
    expect(el('#icon-hint').textContent).toContain("'aaaa' is no longer in the icon list");

    await submit();
    expect(service.update).not.toHaveBeenCalled();
    expect(el('#icon-error').textContent).toContain('Choose an icon');

    await pickIcon('medium_icon');
    await submit();
    expect(service.update).toHaveBeenCalledWith(2, expect.objectContaining({ icon: 'medium_icon' }));
  });

  it('shows not found for an unknown id', async () => {
    await setup('99');
    expect(el('.status.error').textContent).toContain('Category not found');
  });

  it('shows the API message when save is rejected', async () => {
    await setup('2');
    service.update.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 400, error: { detail: "Category name 'Light' is already used." },
    })));

    await type('#name', 'Light');
    await submit();

    expect(el('[role="alert"]').textContent).toContain('already used');
    expect(navigate).not.toHaveBeenCalled();
  });
});
