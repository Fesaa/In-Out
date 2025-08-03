namespace API.Entities;

public class ManualMigration
{
    public int Id { get; set; }
    public string ProductVersion { get; set; }
    public required string Name { get; set; }
    public DateTime RanAt { get; set; }
}