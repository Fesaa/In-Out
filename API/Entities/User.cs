using Microsoft.EntityFrameworkCore;

namespace API.Entities;

[Index(nameof(UserId), IsUnique = true)]
public class User
{
    public int Id { get; set; }
    
    /// <summary>
    /// Subject claim in the OIDC token
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// UserName claim in the OIDC token
    /// </summary>
    public string Name { get; set; }
    public string NormalizedName { get; set; }
    
    public string Language { get; set; }
}