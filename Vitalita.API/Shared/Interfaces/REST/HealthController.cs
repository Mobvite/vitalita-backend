using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vitalita.API.Shared.Interfaces.REST.Resources;

namespace Vitalita.API.Shared.Interfaces.REST;

/// <summary>
///     Liveness probe used by the hosting platform (Render health check).
/// </summary>
/// <remarks>
///     It lives outside <c>api/v1</c> and does not touch the database, so a slow or unavailable database does not
///     make the platform restart a process that is otherwise able to serve requests.
/// </remarks>
[ApiController]
[AllowAnonymous]
[Route("health")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    /// <summary>
    ///     Returns <c>{"status":"Healthy"}</c> when the API process is up.
    /// </summary>
    /// <returns>200 with the health status.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public ActionResult<HealthResource> GetHealth() => Ok(HealthResource.Healthy);
}
