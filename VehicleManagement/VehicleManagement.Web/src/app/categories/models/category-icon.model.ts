// Mirrors VehicleManagement.Application.Dtos.CategoryIconDto: one icon from
// the API's "CategoryIcons" configuration. `name` is what a category stores;
// `imageUrl` is relative to this web app (files in public/) or absolute.
export interface CategoryIcon {
  name: string;
  imageUrl: string;
}
