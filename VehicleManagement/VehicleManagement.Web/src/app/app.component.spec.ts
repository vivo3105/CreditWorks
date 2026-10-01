import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { Component } from '@angular/core';
import { AppComponent } from './app.component';

@Component({ template: '' })
class Blank {}

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideRouter([
          { path: 'vehicles', component: Blank },
          { path: 'vehicles/new', component: Blank },
          { path: 'categories', component: Blank },
        ]),
      ],
    })
      .compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', async () => {
    const fixture = TestBed.createComponent(AppComponent);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Vehicle Management');
  });

  it('shows sidebar links and highlights the current section', async () => {
    const fixture = TestBed.createComponent(AppComponent);
    const harness = await RouterTestingHarness.create();
    const links = () =>
      Array.from(fixture.nativeElement.querySelectorAll('nav a') as NodeListOf<HTMLAnchorElement>);

    await harness.navigateByUrl('/vehicles/new');
    await fixture.whenStable();

    expect(links().map((a) => [a.textContent!.trim(), a.getAttribute('href')])).toEqual([
      ['Manage Vehicle', '/vehicles'],
      ['Manage Category', '/categories'],
    ]);
    // Sub-pages of a section (add/edit vehicle) still highlight that section.
    expect(links()[0].getAttribute('aria-current')).toBe('page');
    expect(links()[1].getAttribute('aria-current')).toBeNull();

    await harness.navigateByUrl('/categories');
    await fixture.whenStable();

    expect(links()[0].getAttribute('aria-current')).toBeNull();
    expect(links()[1].getAttribute('aria-current')).toBe('page');
  });
});
