using AuditIQ.Domain.Entities;

namespace AuditIQ.Application.Scorecards;

/// <summary>
/// The one place that turns "which answer was picked per question" into a score —
/// shared by real evaluations (SubmitEvaluationCommandHandler) and calibration
/// (SaveCalibrationAnswersCommandHandler) so the two can never quietly diverge, the
/// way the client-side percentage displays did before this existed.
/// </summary>
public static class ScoreCalculator
{
    /// <summary>The score a single answer contributes — the chosen option's configured
    /// Value, or null if unanswered. Never client-supplied (QuestionAnswerOption.cs).</summary>
    public static decimal? ResolveAnswerScore(ScorecardQuestion question, string? answerValue) =>
        answerValue is null ? null : question.AnswerOptions.First(o => o.Label == answerValue).Value;

    /// <summary>
    /// Weighted average of the selected answer's Value per question, with two fail-logic
    /// cascades checked across ALL questions before totaling: choosing a Fail All option
    /// zeroes the whole total; choosing a Fail Section option zeroes every OTHER question
    /// in the same SectionName too, not just itself. A Not Applicable answer is NOT excluded
    /// — it contributes its own configured Value (and weight) like any other answer, exactly
    /// as Scorebuddy's scoring does (verified against real migrated evaluation data: Scorebuddy
    /// never excludes N/A from the denominator, it just scores the N/A option like any other —
    /// see Section 5 scoring-parity investigation). Null when nothing scoreable was answered
    /// (denominator is zero).
    ///
    /// <paramref name="maxScore"/> is Scorebuddy's true denominator (Scorecard.MaxScore) —
    /// an independently-configured value that only coincides with the weight sum by convention.
    /// Falls back to the weight sum when null, for scorecards that predate the field.
    /// </summary>
    public static decimal? ComputeTotalScore(IReadOnlyList<ScorecardQuestion> questions, IReadOnlyDictionary<Guid, string?> answerValueByQuestionId, decimal? maxScore = null)
    {
        var selections = questions.ToDictionary(
            q => q.Id,
            q => q.AnswerOptions.FirstOrDefault(o => o.Label == answerValueByQuestionId.GetValueOrDefault(q.Id)));

        var failAll = selections.Values.Any(o => o?.IsFailAll == true);
        if (failAll)
            return 0m;

        var failedSections = questions
            .Where(q => selections[q.Id]?.IsFailSection == true)
            .Select(q => q.SectionName)
            .ToHashSet();

        decimal totalWeight = 0;
        decimal weightedSum = 0;
        foreach (var q in questions)
        {
            var option = selections[q.Id];
            totalWeight += q.Weight;
            var effectiveValue = failedSections.Contains(q.SectionName) ? 0 : (option?.Value ?? 0);
            weightedSum += effectiveValue * q.Weight;
        }

        if (totalWeight <= 0)
            return null;

        var denominator = maxScore ?? totalWeight;
        return denominator > 0 ? Math.Round(weightedSum / denominator, 2) : null;
    }
}
