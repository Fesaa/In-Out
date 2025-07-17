namespace API.DTOs;

public sealed record ClientDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    
    public string CompanyNumber  { get; set; }
    public string InvoiceEmail { get; set; }
    
    public string ContactName  { get; set; }
    public string ContactEmail { get; set; }
    public string ContactNumber  { get; set; }
    
    public bool New { get; set; }
}