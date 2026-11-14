using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.Monitoring.Application.Services;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;
using Vitalita.API.Monitoring.Interfaces.REST.Transform;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

namespace Vitalita.API.Monitoring.Interfaces.REST;

/// <summary>
///     Vital sign measurements of the older adults (US14).
/// </summary>
/// <remarks>
///     Reads: the caregiver in charge, family members with an active access, and administrators. Writes: the
///     caregiver in charge and administrators. An older adult the requester cannot access (or that does not exist)
///     is <c>403</c>, never <c>404</c>. There is no <c>DELETE</c>.
/// </remarks>
/// <param name="commandService">The vital sign command service.</param>
/// <param name="queryService">The vital sign query service.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Tags("Vital Signs")]
public class VitalSignsController(
    IVitalSignCommandService commandService,
    IVitalSignQueryService queryService,
    IStringLocalizer<SharedResource> localizer)
    : ControllerBase
{
    private const string NotFoundKey = "VitalSignNotFound";

    /// <summary>
    ///     Lists the vital signs of an older adult, or every vital sign (administrators only).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>?olderAdultId=</c>: the vital signs of that older adult (<c>403</c> when the requester cannot read it).
    ///     </para>
    ///     <para>Without <c>olderAdultId</c>: every vital sign, administrators only.</para>
    /// </remarks>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching vital signs (a plain array; empty when there are none).</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VitalSignResource>), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetVitalSigns([FromQuery] int? olderAdultId,
        CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var result = olderAdultId is { } value
            ? await queryService.Handle(new GetVitalSignsByOlderAdultIdQuery(requester, value), cancellationToken)
            : await queryService.Handle(new GetAllVitalSignsQuery(requester), cancellationToken);
        return result.Fold<ActionResult>(
            records => Ok(records.Select(VitalSignResourceFromEntityAssembler.ToResourceFromEntity)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Gets a vital sign by identifier.
    /// </summary>
    /// <remarks><c>404</c> when it does not exist; <c>403</c> when the requester cannot read its older adult.</remarks>
    /// <param name="id">Measurement identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The vital sign.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VitalSignResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetVitalSignById(int id, CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var result = await queryService.Handle(new GetVitalSignByIdQuery(requester, id), cancellationToken);
        return result.Fold<ActionResult>(
            record => Ok(VitalSignResourceFromEntityAssembler.ToResourceFromEntity(record)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Records a vital sign measurement.
    /// </summary>
    /// <remarks>
    ///     Only the caregiver in charge (or an administrator). The value must be inside the range of its type;
    ///     blood pressure needs <c>secondaryValue</c> (diastolic) lower than <c>value</c> (systolic). The unit is derived
    ///     from the type. <c>measuredAt</c> cannot be in the future (5 minutes of tolerance).
    /// </remarks>
    /// <param name="resource">The vital sign data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created vital sign, with its location.</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(VitalSignResource), StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> CreateVitalSign([FromBody] SaveVitalSignResource resource,
        CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var command = CreateVitalSignCommandFromResourceAssembler.ToCommandFromResource(requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            record => CreatedAtAction(nameof(GetVitalSignById), new { id = record.Id },
                VitalSignResourceFromEntityAssembler.ToResourceFromEntity(record)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, Enum.Parse<VitalSignType>(resource.Type), this, localizer));
    }

    /// <summary>
    ///     Corrects a vital sign measurement.
    /// </summary>
    /// <remarks>
    ///     Only the caregiver in charge (or an administrator). <c>olderAdultId</c> cannot change (<c>409</c>).
    /// </remarks>
    /// <param name="id">Measurement identifier.</param>
    /// <param name="resource">The whole vital sign.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated vital sign.</returns>
    [HttpPut("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(VitalSignResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> UpdateVitalSign(int id, [FromBody] SaveVitalSignResource resource,
        CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var command = UpdateVitalSignCommandFromResourceAssembler.ToCommandFromResource(id, requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            record => Ok(VitalSignResourceFromEntityAssembler.ToResourceFromEntity(record)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, Enum.Parse<VitalSignType>(resource.Type), this, localizer));
    }
}
