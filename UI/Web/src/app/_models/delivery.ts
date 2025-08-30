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
  systemMessages: SystemMessage[],
  lines: DeliveryLine[],

  createdUtc?: Date,
  lastModifiedUtc?: Date,
}

export type SystemMessage = {
  message: string,
  createdUtc: Date,
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

export const AllDeliveryStates: DeliveryState[] = Object.values(DeliveryState).filter(
  (v): v is DeliveryState => typeof v === 'number'
);
