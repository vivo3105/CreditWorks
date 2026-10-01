import { Routes } from '@angular/router';
import { CategoryFormComponent } from './categories/components/category-form/category-form.component';
import { CategoryListComponent } from './categories/components/category-list/category-list.component';
import { VehicleFormComponent } from './vehicles/components/vehicle-form/vehicle-form.component';
import { VehicleListComponent } from './vehicles/components/vehicle-list/vehicle-list.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'vehicles' },
  { path: 'vehicles', component: VehicleListComponent },
  { path: 'vehicles/new', component: VehicleFormComponent },
  { path: 'vehicles/:id/edit', component: VehicleFormComponent },
  { path: 'categories', component: CategoryListComponent },
  { path: 'categories/new', component: CategoryFormComponent },
  { path: 'categories/:id/edit', component: CategoryFormComponent },
  { path: '**', redirectTo: 'vehicles' },
];
