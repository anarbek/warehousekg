using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Application.Features.PreOrders.Commands;
using WarehouseKG.Application.Features.PreOrders.Dtos;
using WarehouseKG.Application.Features.PreOrders.Queries;

namespace WarehouseKG.Api.Controllers;

[Route("api/v1/payment-types")]
public class PaymentTypesController : ApiControllerBase
{
    private readonly ISender _sender;

    public PaymentTypesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Policy = "payment-types:read")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentTypeDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPaymentTypesQuery(), cancellationToken));
}
