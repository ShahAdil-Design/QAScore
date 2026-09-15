namespace AuditIQ.Application.Scorecards.Commands;

/// <summary>One selectable answer plus its scoring configuration. Value is the score
/// awarded when chosen; the three flags are fail-logic cascades checked at submit time
/// (QuestionAnswerOption.cs carries the full contract/validation convention).</summary>
public sealed record AnswerOptionInput(string Label, decimal Value, bool IsFailSection, bool IsFailAll, bool IsNotApplicable);

/// <summary>Shared write-side shape for a question + its answer options, used by both
/// CreateScorecard and UpdateScorecard — a scorecard's questions are edited as a whole
/// aggregate, matching how the admin builder UI submits the full structure each time.</summary>
public sealed record ScorecardQuestionInput(
    string SectionName,
    string Text,
    int Weight,
    bool IsFailLogic,
    int SortOrder,
    IReadOnlyList<AnswerOptionInput> AnswerOptions);
