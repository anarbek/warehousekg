using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Common;

namespace WarehouseKG.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Maps a workflow <see cref="OperationResult"/> to an HTTP response.</summary>
    protected IActionResult MapWorkflowResult(OperationResult result) => result switch
    {
        OperationResult.Success => NoContent(),
        OperationResult.NotFound => NotFound(),
        OperationResult.InvalidState => Conflict(new { error = "The operation is not in a state that allows this action." }),
        _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
}
