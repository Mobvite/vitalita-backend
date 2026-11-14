using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Vitalita.API.Monitoring.Application.Services;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;
using Vitalita.API.Monitoring.Interfaces.REST.Transform;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

namespace Vitalita.API.Monitoring.Interfaces.REST;

/// <summary>
///     Medications of the older adults and their daily schedule.
/// </summary>
/// <remarks>
///     Reads: the caregiver in charge, family members with an active access, and administrators. Writes: the
///     caregiver in charge and administrators. An older adult the requester cannot access (or that does not exist)
///     is <c>403</c>, never <c>404</c>. There is no <c>DELETE</c>: a medication is deactivated
///     with <c>PUT</c> and <c>active: false</c>.
/// </remarks>
/// <param name="commandService">The medication command service.</param>
/// <param name="queryService">The medication query service.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Tags("Medications")]
public class MedicationsController(
    IMedicationCommandService commandService,
    IMedicationQueryService queryService,
    IStringLocalizer<SharedResource> localizer)
    : ControllerBase
{
    private const string NotFoundKey = "MedicationNotFound";

    /// <summary>
    ///     Lists the medications of an older adult, or every medication (administrators only).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>?olderAdultId=</c>: the medications of that older adult (<c>403</c> when the requester cannot read it).
    ///     </para>
    ///     <para>Without <c>olderAdultId</c>: every medication, administrators only.</para>
    ///     <para><c>active</c> filters by the active flag in both cases.</para>
    /// </remarks>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="active">Optional filter: <c>true</c> or <c>false</c>.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching medications (a plain array; empty when there are none).</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MedicationResource>), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetMedications([FromQuery] int? olderAdultId, [FromQuery] bool? active,
        CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var result = olderAdultId is { } value
            ? await queryService.Handle(new GetMedicationsByOlderAdultIdQuery(requester, value, active), cancellationToken)
            : await queryService.Handle(new GetAllMedicationsQuery(requester, active), cancellationToken);
        return result.Fold<ActionResult>(
            records => Ok(records.Select(MedicationResourceFromEntityAssembler.ToResourceFromEntity)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Gets a medication by identifier.
    /// </summary>
    /// <remarks><c>404</c> when it does not exist; <c>403</c> when the requester cannot read its older adult.</remarks>
    /// <param name="id">Medication identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The medication.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MedicationResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetMedicationById(int id, CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var result = await queryService.Handle(new GetMedicationByIdQuery(requester, id), cancellationToken);
        return result.Fold<ActionResult>(
            record => Ok(MedicationResourceFromEntityAssembler.ToResourceFromEntity(record)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Registers a medication.
    /// </summary>
    /// <remarks>
    ///     Only the caregiver in charge (or an administrator). <c>scheduleTimes</c> has 1 to 12 wall-clock
    ///     <c>HH:mm</c> times, stored sorted and without duplicates. <c>active</c> defaults to <c>true</c>.
    /// </remarks>
    /// <param name="resource">The medication data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created medication, with its location.</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(MedicationResource), StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> CreateMedication([FromBody] SaveMedicationResource resource,
        CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var command = CreateMedicationCommandFromResourceAssembler.ToCommandFromResource(requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            record => CreatedAtAction(nameof(GetMedicationById), new { id = record.Id },
                MedicationResourceFromEntityAssembler.ToResourceFromEntity(record)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, this, localizer));
    }

    /// <summary>
    ///     Replaces a medication (also to deactivate it).
    /// </summary>
    /// <remarks>
    ///     Only the caregiver in charge (or an administrator). <c>olderAdultId</c> cannot change (<c>409</c>). Send <c>active: false</c> to
    ///     deactivate it.
    /// </remarks>
    /// <param name="id">Medication identifier.</param>
    /// <param name="resource">The whole medication.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated medication.</returns>
    [HttpPut("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(MedicationResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
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
    public async Task<ActionResult> UpdateMedication(int id, [FromBody] SaveMedicationResource resource,
        CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var command = UpdateMedicationCommandFromResourceAssembler.ToCommandFromResource(id, requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            record => Ok(MedicationResourceFromEntityAssembler.ToResourceFromEntity(record)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, this, localizer));
    }
}
