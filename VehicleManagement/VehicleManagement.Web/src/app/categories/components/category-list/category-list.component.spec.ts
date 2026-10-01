import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Category } from '../../models/category.model';
import { CategoryService } from '../../services/category.service';
import { CategoryListComponent } from './category-list.component';

const categories: Category[] = [
  { id: 1, name: 'Light', minWeight: 0, maxWeight: 499.99, icon: 'light_icon' },
  { id: 2, name: 'Medium', minWeight: 500, maxWeight: 2499.99, icon: 'medium_icon' },
  { id: 3, name: 'Heavy', minWeight: 2500, maxWeight: null, icon: 'heavy_icon' },
];

describe('CategoryListComponent', () => {
  let fixture: ComponentFixture<CategoryListComponent>;
  let service: { getAll: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn>; getIcons: ReturnType<typeof vi.fn> };

  const el = () => fixture.nativeElement as HTMLElement;
  const rows = () =>
    Array.from(el().querySelectorAll('tbody tr')).map((row) =>
      Array.from(row.querySelectorAll('td')).slice(0, 3).map((td) => td.textContent!.trim()),
    );
  const deleteButton = (name: string) =>
    el().querySelector(`button[aria-label="Delete category ${name}"]`) as HTMLButtonElement;

  const render = async (getAll: ReturnType<typeof vi.fn>) => {
    service = {
      getAll,
      delete: vi.fn(() => of(undefined)),
      getIcons: vi.fn(() => of([
        { name: 'light_icon', imageUrl: 'icons/light.svg' },
        { name: 'medium_icon', imageUrl: 'icons/medium.svg' },
        { name: 'heavy_icon', imageUrl: 'icons/heavy.svg' },
      ])),
    };

    await TestBed.configureTestingModule({
      imports: [CategoryListComponent],
      providers: [provideRouter([]), { provide: CategoryService, useValue: service }],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryListComponent);
    await fixture.whenStable();
  };

  afterEach(() => vi.restoreAllMocks());

  it('lists categories with their weight ranges and no gap notice when fully covered', async () => {
    await render(vi.fn(() => of(categories)));

    expect(rows()).toEqual([
      ['Light', '0', '499.99'],
      ['Medium', '500', '2,499.99'],
      ['Heavy', '2,500', 'No limit'],
    ]);
    expect(el().querySelector('.gaps')).toBeNull();
  });

  it('shows each icon image next to its name and flags icons no longer configured', async () => {
    await render(vi.fn(() => of([categories[0], { ...categories[1], icon: 'aaaa' }])));

    const iconCells = Array.from(el().querySelectorAll('tbody tr')).map((row) => row.querySelectorAll('td')[3]);

    expect(iconCells[0].querySelector('img')?.getAttribute('src')).toBe('icons/light.svg');
    expect(iconCells[0].textContent).not.toContain('not in icon list');
    expect(iconCells[1].querySelector('img')).toBeNull();
    expect(iconCells[1].textContent).toContain('aaaa');
    expect(iconCells[1].textContent).toContain('(not in icon list)');
  });

  it('lists the weights no category covers', async () => {
    await render(vi.fn(() => of([categories[0], { ...categories[2], maxWeight: 4999.99 }])));

    expect(el().querySelector('.gaps')?.textContent).toContain('500 – 2,499.99 kg, 5,000 kg and above');
  });

  it('shows an error when loading fails', async () => {
    await render(vi.fn(() => throwError(() => new HttpErrorResponse({ status: 0 }))));

    expect(el().querySelector('.status.error')?.textContent).toContain('Could not reach the API');
    expect(el().querySelector('table')).toBeNull();
  });

  it('links Add Category and the edit icon to the form routes', async () => {
    await render(vi.fn(() => of(categories)));

    expect(el().querySelector('a.btn-primary')!.getAttribute('href')).toBe('/categories/new');
    expect(el().querySelector('a[aria-label="Edit category Medium"]')!.getAttribute('href'))
      .toBe('/categories/2/edit');
  });

  it('confirms, deletes just that category, then shows the gap it left', async () => {
    await render(vi.fn()
      .mockReturnValueOnce(of(categories))
      .mockReturnValueOnce(of([categories[0], categories[2]])));
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);

    deleteButton('Medium').click();
    await fixture.whenStable();

    expect(confirmSpy.mock.calls[0][0]).toContain('Vehicles weighing 500 – 2,499.99 kg will be uncategorised.');
    expect(service.delete).toHaveBeenCalledWith(2);
    expect(rows().map((r) => r[0])).toEqual(['Light', 'Heavy']);
    expect(el().querySelector('.gaps')?.textContent).toContain('500 – 2,499.99 kg');
  });

  it('does nothing when the confirmation is cancelled', async () => {
    await render(vi.fn(() => of(categories)));
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    deleteButton('Medium').click();
    await fixture.whenStable();

    expect(service.delete).not.toHaveBeenCalled();
    expect(rows().length).toBe(3);
  });

  it('keeps the row and shows the API message when delete fails', async () => {
    await render(vi.fn(() => of(categories)));
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    service.delete.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 500, error: { detail: 'Database unavailable.' },
    })));

    deleteButton('Medium').click();
    await fixture.whenStable();

    expect(rows().length).toBe(3);
    expect(el().querySelector('[role="alert"]')?.textContent).toContain('Database unavailable.');
  });
});
