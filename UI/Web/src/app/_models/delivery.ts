import {User} from './user';
import {Client} from './client';

export type Delivery = {
  id: number,
  state: DeliveryState,

  fromId: number,
  from?: User,

  clientId: number,
  recipient?: Client,

  message: string,
  systemMessages: string[],
  lines: DeliveryLine[],

  createdUtc?: Date,
  lastModifiedUtc?: Date,
}

export type DeliveryLine = {
  productId: number,
  quantity: number,
}

export enum DeliveryState {
  InProgress = 0,
  Completed = 1,
  Handled = 2,
  Cancelled = 3,
}
