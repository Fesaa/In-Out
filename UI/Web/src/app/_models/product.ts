export type Product = {
  id: number;
  name: string;
  description: string;
  categoryId: number;
  sortValue: number;
  type: ProductType;
  isTracked: boolean;
  enabled: boolean;
  prices: Record<string, number>;
};

export enum ProductType {
  Consumable = 0,
  OneTime = 1,
}

export const AllProductTypes = [ProductType.Consumable, ProductType.OneTime];

export type ProductCategory = {
  id: number;
  name: string;
  enabled: boolean;
  autoCollapse: boolean;
  sortValue: number;
}

export type PriceCategory = {
  id: number;
  name: string;
}
