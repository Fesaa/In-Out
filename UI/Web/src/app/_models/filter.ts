
export type Filter = {
  statements: FilterStatement[];
  combination: FilterCombination;
  sortOptions: SortOptions;
  limit: number;
}

export type SortOptions = {
  sortField: SortField;
  isAscending: boolean;
}

export enum SortField {
  None = 0,
}

export type FilterStatement = {
  comparison: FilterComparison,
  field: FilterField,
  value: string;
}

export enum FilterField {
  DeliveryState = 0,
  From = 1,
  Recipient = 2,
  Lines = 3,
  Products = 4,
  Created = 5,
  LastModified = 6,
}

export const AllFilterFields: FilterField[] = Object.values(FilterField).filter(
  (v): v is FilterField => typeof v === 'number'
);

export enum FilterComparison {
  Contains = 0,
  NotContains = 9,
  Equals = 1,
  NotEquals = 2,
  StartsWith = 3,
  EndsWith = 4,
  GreaterThan = 5,
  GreaterThanOrEquals = 6,
  LessThan = 7,
  LessThanOrEquals = 8,
}

export const AllFilterComparisons: FilterComparison[] = Object.values(FilterComparison).filter(
  (v): v is FilterComparison => typeof v === 'number'
);

export enum FilterCombination {
  Or = 0,
  And = 1,
}

export enum FilterInputType {
  FreeText = 0,
  FreeNumber = 1,
  Typeahead = 2,
  Date = 3,
}

export function serializeFilterToQuery(filter: Filter): string {
  const statements = filter.statements
    .map(s => `${s.field},${s.comparison},${encodeURIComponent(s.value)}`)
    .join(';');

  const combination = FilterCombination[filter.combination];
  const limit = filter.limit;
  const sortField = filter.sortOptions.sortField;
  const isAscending = filter.sortOptions.isAscending;

  const params = new URLSearchParams();
  params.set('statements', statements);
  params.set('combination', combination);
  params.set('limit', limit.toString());
  params.set('sortField', sortField.toString());
  params.set('isAscending', isAscending.toString());

  return params.toString();
}

export function deserializeFilterFromQuery(query: string): Filter {
  const params = new URLSearchParams(query);

  const rawStatements = params.get('statements') || '';
  const combinationStr = params.get('combination') || 'And';
  const limitStr = params.get('limit') || '50';
  const sortField = params.get('sortField') || '';
  const isAscending = params.get('isAscending') || '';

  const statements: FilterStatement[] = rawStatements
    .split(';')
    .filter(Boolean)
    .map(statement => {
      const [fieldStr, comparisonStr, valueEncoded] = statement.split(',');
      return {
        field: Number(fieldStr) as FilterField,
        comparison: Number(comparisonStr) as FilterComparison,
        value: decodeURIComponent(valueEncoded),
      };
    });

  return {
    statements,
    combination: FilterCombination[combinationStr as keyof typeof FilterCombination],
    limit: Number(limitStr),
    sortOptions: {
      sortField: sortField ? parseInt(sortField) as SortField : SortField.None,
      isAscending: isAscending === 'true',
    }
  };
}


