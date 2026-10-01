import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { Manufacturer } from '../models/manufacturer.model';

@Injectable({ providedIn: 'root' })
export class ManufacturerService {
  private readonly api = inject(ApiService);

  getAll(): Observable<Manufacturer[]> {
    return this.api.get<Manufacturer[]>('manufacturers');
  }
}
