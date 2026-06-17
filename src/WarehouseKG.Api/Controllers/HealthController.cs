using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseKG.Api.Authorization;

namespace WarehouseKG.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.RequireViewer)]
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });
}
