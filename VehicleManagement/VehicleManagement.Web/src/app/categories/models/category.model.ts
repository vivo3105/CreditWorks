// Mirrors VehicleManagement.Application.Dtos.VehicleCategoryDto.
export interface Category {
  id: number;
  name: string;
  minWeight: number;
  // null for the open-ended top category.
  maxWeight: number | null;
  icon: string;
}

// Mirrors VehicleManagement.Application.Dtos.SaveVehicleCategoryRequest
// (create and update). maxWeight null = no upper limit.
export interface SaveCategoryRequest {
  name: string;
  minWeight: number;
  maxWeight: number | null;
  icon: string;
}
