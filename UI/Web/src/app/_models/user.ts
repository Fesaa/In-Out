import {Role} from '../_services/auth.service';

export type User = {
  id: number,
  name: string,
  language: string,
  roles: Role[],
}

export const AllLanguages = ['en', 'nl'];
