using System.Net;
using System.Text.Json;
using API.Exceptions;

namespace API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var errorMessage = string.IsNullOrEmpty(ex.Message) ? "Internal Server Error" : ex.Message;
            var statusCode = (int) HttpStatusCode.InternalServerError;
            
            if (ex is InOutException)
            {
                if (!ex.Message.StartsWith("errors."))
                {
                    errorMessage = "errors." + errorMessage;
                }

                statusCode = (int)HttpStatusCode.BadRequest;
            }
            
            logger.LogDebug(ex, "An exception occurred while handling an http request.");
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiException(context.Response.StatusCode, errorMessage, ex.StackTrace);
            var json = JsonSerializer.Serialize(response, JsonSerializerOptions);

            await context.Response.WriteAsync(json);
        }
    }
}