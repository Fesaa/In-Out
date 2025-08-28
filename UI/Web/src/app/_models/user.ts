import {Role} from '../_services/auth.service';

export type User = {
  id: number,
  name: string,
  roles: Role[],
}
