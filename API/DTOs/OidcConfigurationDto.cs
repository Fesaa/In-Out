using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public sealed record OidcConfigurationDto
{
    [Required]
    public string Authority { get; init; }
    
    [Required]
    public string ClientId { get; init; }
    
    [Required]
    public string ClientSecret { get; init; }

    public bool ValidConfig()
    {
        return !string.IsNullOrWhiteSpace(Authority)
               && !string.IsNullOrWhiteSpace(ClientId)
               && !string.IsNullOrWhiteSpace(ClientSecret);
    }

}