import {Product} from './product';


export type Stock = {
  id: number;
  productId: number;
  product: Product;
  quantity: number;

  name: string;
  description: string;

}

export type StockHistory = {
  id: number;
  stockId: number;
  userId: number;

  operation: StockOperation;
  value: number;
  quantityBefore: number;
  quantityAfter: number;
  referenceNumber?: string;
  notes?: string;

  createdUtc: Date,
  lastModifiedUtc: Date,
}

export enum StockOperation {
  Add = 0,
  Remove = 1,
  Set = 2,
}
