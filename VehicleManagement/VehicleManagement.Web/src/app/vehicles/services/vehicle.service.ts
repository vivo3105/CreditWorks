import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { SaveVehicleRequest, Vehicle } from '../models/vehicle.model';

@Injectable({ providedIn: 'root' })
export class VehicleService {
  private readonly api = inject(ApiService);

  getAll(): Observable<Vehicle[]> {
    return this.api.get<Vehicle[]>('vehicles');
  }

  getById(id: number): Observable<Vehicle> {
    return this.api.get<Vehicle>(`vehicles/${id}`);
  }

  create(request: SaveVehicleRequest): Observable<Vehicle> {
    return this.api.post<Vehicle>('vehicles', request);
  }

  update(id: number, request: SaveVehicleRequest): Observable<Vehicle> {
    return this.api.put<Vehicle>(`vehicles/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.api.delete(`vehicles/${id}`);
  }
}
