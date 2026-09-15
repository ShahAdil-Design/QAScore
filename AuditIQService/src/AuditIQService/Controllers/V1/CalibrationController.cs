using Asp.Versioning;
using AuditIQ.Api.Auth;
using AuditIQ.Api.Contracts.Requests;
using AuditIQ.Api.Extensions;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Calibration.Commands.AddEvaluationsToCalibrationList;
using AuditIQ.Application.Calibration.Commands.CreateCalibrationList;
using AuditIQ.Application.Calibration.Commands.SaveCalibrationAnswers;
using AuditIQ.Application.Calibration.Queries.GetCalibrationItemDetail;
using AuditIQ.Application.Calibration.Queries.GetCalibrationListItems;
using AuditIQ.Application.Calibration.Queries.GetCalibrationLists;
using AuditIQ.Application.Calibration.Queries.GetEvaluationsForCalibration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditIQ.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/calibration-lists")]
[Authorize(Policy = AuthorizationPolicies.RequireEvaluator)]
public class CalibrationController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLists(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetCalibrationListsQuery(search, page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCalibrationListRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCalibrationListCommand(request.Name, request.VisibilityScope, request.CreatedByUserId), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetItems), new { id = result.Value, version = "1.0" }, new { id = result.Value })
            : result.ToActionResult(this);
    }

    /// <summary>Candidate evaluations for the list builder's Filter Results panel + table
    /// (manual multi-select browse, or a Random sample when random=true).</summary>
    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidates(
        [FromQuery] DateTimeOffset? dateFrom,
        [FromQuery] DateTimeOffset? dateTo,
        [FromQuery] Guid? groupId,
        [FromQuery] Guid? teamId,
        [FromQuery] Guid? eventTypeId,
        [FromQuery] Guid? evaluatorId,
        [FromQuery] string? reference,
        [FromQuery] Guid? scorecardId,
        [FromQuery] Guid? categoryId,
        [FromQuery] decimal? scoreMin,
        [FromQuery] decimal? scoreMax,
        [FromQuery] bool random = false,
        [FromQuery] int sampleSize = 50,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEvaluationsForCalibrationQuery(
            dateFrom, dateTo, groupId, teamId, eventTypeId, evaluatorId, reference,
            scorecardId, categoryId, scoreMin, scoreMax, random, sampleSize, page, pageSize);
        var result = await sender.Send(query, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItems(Guid id, [FromBody] AddEvaluationsToCalibrationListRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddEvaluationsToCalibrationListCommand(id, request.EvaluationIds), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}/items")]
    public async Task<IActionResult> GetItems(
        Guid id,
        [FromQuery] Guid? evaluatorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetCalibrationListItemsQuery(id, evaluatorId, page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("items/{itemId:guid}")]
    public async Task<IActionResult> GetItemDetail(Guid itemId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCalibrationItemDetailQuery(itemId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("items/{itemId:guid}/answers")]
    public async Task<IActionResult> SaveAnswers(Guid itemId, [FromBody] SaveCalibrationAnswersRequest request, CancellationToken cancellationToken)
    {
        var answers = request.Answers
            .Select(a => new CalibrationAnswerInput(a.QuestionId, a.AnswerValue, a.CauseCode, a.Comment))
            .ToList();

        var result = await sender.Send(new SaveCalibrationAnswersCommand(itemId, request.EvaluatorId, answers), cancellationToken);
        return result.ToActionResult(this);
    }
}
