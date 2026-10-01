// Mirrors VehicleManagement.Application.Dtos.VehicleDto.
export interface Vehicle {
  id: number;
  ownerName: string;
  manufacturerId: number;
  manufacturerName: string;
  yearOfManufacture: number;
  weight: number;
  // null when the weight falls in a gap between categories (uncategorised).
  categoryName: string | null;
  categoryIcon: string | null;
}

// Mirrors VehicleManagement.Application.Dtos.SaveVehicleRequest (create and update).
export interface SaveVehicleRequest {
  ownerName: string;
  manufacturerId: number;
  yearOfManufacture: number;
  weight: number;
}
