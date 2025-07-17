using Microsoft.EntityFrameworkCore;

namespace API.Entities;

[Index(nameof(UserId), IsUnique = true)]
public class UserPreferences
{
    public int Id { get; set; }
    
    /**
     * The ID given from OIDC
     */
    public string UserId { get; set; }
    
    public string Language { get; set; }
}