using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;
using WarehouseKG.Application.Features.Customers.Commands;
using WarehouseKG.Application.Features.Customers.Dtos;
using WarehouseKG.Application.Features.Customers.Queries;

namespace WarehouseKG.Api.Controllers;

/// <summary>
/// Manages customers for the current tenant.
/// </summary>
[ApiController]
[Route("api/v1/customers")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all customers.</summary>
    [HttpGet]
    [Authorize(Policy = "customers:read")]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetCustomersQuery(), cancellationToken));

    /// <summary>Returns a single customer by id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "customers:read")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new customer.</summary>
    [HttpPost]
    [Authorize(Policy = "customers:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Updates an existing customer.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "customers:write")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.Code,
            request.Name,
            request.ContactName,
            request.Email,
            request.Phone,
            request.Address,
            request.TaxId,
            request.Latitude,
            request.Longitude,
            request.IsActive);

        var updated = await _sender.Send(command, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a customer.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "customers:delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteCustomerCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

/// <summary>Request body for updating a customer.</summary>
public record UpdateCustomerRequest(
    string Code,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? TaxId,
    double? Latitude,
    double? Longitude,
    bool IsActive);
