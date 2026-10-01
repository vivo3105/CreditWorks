import { Component, computed, inject, input } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { CategoryService } from '../../../categories/services/category.service';

// Shows the image configured for a category icon name. Renders nothing when
// the name is empty, isn't in the configured list, or the list can't load,
// so callers can always place it next to the category name.
@Component({
  selector: 'app-category-icon',
  template: `
    @if (src(); as url) {
      <img [src]="url" [alt]="alt()" [width]="size()" [height]="size()" />
    }
  `,
  styles: `
    :host { display: inline-flex; align-items: center; }
    img { display: block; object-fit: contain; }
  `,
})
export class CategoryIconComponent {
  private readonly icons = toSignal(
    inject(CategoryService).getIcons().pipe(catchError(() => of([]))),
    { initialValue: [] },
  );

  readonly name = input<string | null | undefined>();
  readonly size = input(20);
  // Decorative next to a visible category name by default.
  readonly alt = input('');

  protected readonly src = computed(() => {
    const name = this.name();
    return name ? (this.icons().find((i) => i.name === name)?.imageUrl ?? null) : null;
  });
}
