
export enum ExportKind {
  Csv = 0,
}

export interface ExportRequest {
  kind: ExportKind;
  deliveryIds: number[];
}
