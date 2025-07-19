namespace API.Entities.Interfaces;

public interface IEntityDate
{
    DateTime CreatedUtc { get; set; }
    DateTime LastModifiedUtc { get; set; }
}