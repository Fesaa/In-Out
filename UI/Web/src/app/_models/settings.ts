
export enum LogLevel {
  Verbose = 0,
  Debug = 1,
  Information = 2,
  Warning = 3,
  Error = 4,
  Fatal = 5
}

export const LogLevelValues = [
  LogLevel.Verbose,
  LogLevel.Debug,
  LogLevel.Information,
  LogLevel.Warning,
  LogLevel.Error,
  LogLevel.Fatal
];


export enum DeliveryExportField {
  Id = 0,
  State = 1,
  FromId = 2,
  From = 3,
  RecipientId = 4,
  RecipientName = 5,
  RecipientEmail = 6,
  CompanyNumber = 7,
  Message = 8,
  Products = 9,
  CreatedUtc = 10,
  LastModifiedUtc = 11
}

export const DeliveryExportFieldValues = [
  DeliveryExportField.Id,
  DeliveryExportField.State,
  DeliveryExportField.FromId,
  DeliveryExportField.From,
  DeliveryExportField.RecipientId,
  DeliveryExportField.RecipientName,
  DeliveryExportField.RecipientEmail,
  DeliveryExportField.CompanyNumber,
  DeliveryExportField.Message,
  DeliveryExportField.Products,
  DeliveryExportField.CreatedUtc,
  DeliveryExportField.LastModifiedUtc
];



export type CsvExportConfiguration = {
  headerNames: string[],
  headerOrder: DeliveryExportField[],
}

export type Settings = {
  logLevel: LogLevel;
  csvExportConfiguration: CsvExportConfiguration
}
