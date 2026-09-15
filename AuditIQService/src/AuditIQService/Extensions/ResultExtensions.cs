using AuditIQ.Application.Abstractions.Results;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Extensions;

/// <summary>Maps a handler's Result/Result&lt;T&gt; onto the matching HTTP response.</summary>
public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        return result.IsSuccess ? controller.NoContent() : Problem(result.FirstError, controller);
    }

    public static IActionResult ToActionResult<TValue>(this Result<TValue> result, ControllerBase controller)
    {
        return result.IsSuccess ? controller.Ok(result.Value) : Problem(result.FirstError, controller);
    }

    private static IActionResult Problem(Error error, ControllerBase controller)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return controller.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode);
    }
}
