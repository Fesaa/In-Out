namespace API.Exceptions;

public record ApiException(int Status, string? Message = null, string? Details = null);