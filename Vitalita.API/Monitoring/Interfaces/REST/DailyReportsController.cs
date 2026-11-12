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
///     Daily reports of the older adults (US13).
/// </summary>
/// <remarks>
///     Reads: the caregiver in charge, family members with an active access, and administrators. Writes: the
///     caregiver in charge and administrators. An older adult the requester cannot access (or that does not exist)
///     is <c>403</c>, never <c>404</c>. There is no <c>DELETE</c>.
/// </remarks>
/// <param name="commandService">The daily report command service.</param>
/// <param name="queryService">The daily report query service.</param>
/// <param name="localizer">String localizer for response messages.</param>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Tags("Daily Reports")]
public class DailyReportsController(
    IDailyReportCommandService commandService,
    IDailyReportQueryService queryService,
    IStringLocalizer<SharedResource> localizer)
    : ControllerBase
{
    private const string NotFoundKey = "DailyReportNotFound";

    /// <summary>
    ///     Lists the daily reports of an older adult, or every daily report (administrators only).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>?olderAdultId=</c>: the daily reports of that older adult (<c>403</c> when the requester cannot read it).
    ///     </para>
    ///     <para>Without <c>olderAdultId</c>: every daily report, administrators only.</para>
    /// </remarks>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching daily reports (a plain array; empty when there are none).</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DailyReportResource>), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetDailyReports([FromQuery] int? olderAdultId,
        CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var result = olderAdultId is { } value
            ? await queryService.Handle(new GetDailyReportsByOlderAdultIdQuery(requester, value), cancellationToken)
            : await queryService.Handle(new GetAllDailyReportsQuery(requester), cancellationToken);
        return result.Fold<ActionResult>(
            records => Ok(records.Select(DailyReportResourceFromEntityAssembler.ToResourceFromEntity)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Gets a daily report by identifier.
    /// </summary>
    /// <remarks><c>404</c> when it does not exist; <c>403</c> when the requester cannot read its older adult.</remarks>
    /// <param name="id">Report identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The daily report.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DailyReportResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> GetDailyReportById(int id, CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var result = await queryService.Handle(new GetDailyReportByIdQuery(requester, id), cancellationToken);
        return result.Fold<ActionResult>(
            record => Ok(DailyReportResourceFromEntityAssembler.ToResourceFromEntity(record)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, NotFoundKey, this, localizer));
    }

    /// <summary>
    ///     Records a daily report.
    /// </summary>
    /// <remarks>
    ///     Only the caregiver in charge (or an administrator). <c>recordedByUserId</c> is the token user
    ///     (the body value is ignored). <c>reportDate</c> is a UTC instant and cannot be in the future (5 minutes of
    ///     tolerance).
    /// </remarks>
    /// <param name="resource">The daily report data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created daily report, with its location.</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DailyReportResource), StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden,
        MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError,
        MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> CreateDailyReport([FromBody] SaveDailyReportResource resource,
        CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var command = CreateDailyReportCommandFromResourceAssembler.ToCommandFromResource(requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            record => CreatedAtAction(nameof(GetDailyReportById), new { id = record.Id },
                DailyReportResourceFromEntityAssembler.ToResourceFromEntity(record)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, this, localizer));
    }

    /// <summary>
    ///     Replaces a daily report.
    /// </summary>
    /// <remarks>
    ///     Only the caregiver in charge (or an administrator). <c>olderAdultId</c> cannot change (<c>409</c>). The original author is kept.
    /// </remarks>
    /// <param name="id">Report identifier.</param>
    /// <param name="resource">The whole daily report.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated daily report.</returns>
    [HttpPut("{id:int}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DailyReportResource), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
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
    public async Task<ActionResult> UpdateDailyReport(int id, [FromBody] SaveDailyReportResource resource,
        CancellationToken cancellationToken)
    {
        if (User.ToRequester() is not { } requester) return Challenge();

        var command = UpdateDailyReportCommandFromResourceAssembler.ToCommandFromResource(id, requester, resource);
        var result = await commandService.Handle(command, cancellationToken);
        return result.Fold<ActionResult>(
            record => Ok(DailyReportResourceFromEntityAssembler.ToResourceFromEntity(record)),
            error => ActionResultFromMonitoringErrorAssembler.ToActionResult(error, this, localizer));
    }
}
