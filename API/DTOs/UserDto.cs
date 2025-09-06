namespace API.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Language { get; set; }
    public IList<string> Roles { get; set; } = [];
}