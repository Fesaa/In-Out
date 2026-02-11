namespace API.Entities;

public class Client
{
    public int  Id { get; set; }
    public required string Name { get; set; }
    public string NormalizedName { get; set; }
    public string Address { get; set; }

    public string CompanyNumber  { get; set; }
    public string InvoiceEmail { get; set; }

    public string ContactName  { get; set; }
    public string ContactEmail { get; set; }
    public string ContactNumber  { get; set; }

    public int? DefaultPriceCategoryId { get; set; }
    public PriceCategory? DefaultPriceCategory { get; set; }
}
