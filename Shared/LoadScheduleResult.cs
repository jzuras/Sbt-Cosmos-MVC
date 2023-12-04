namespace Sbt.Shared;

public record LoadScheduleResult
{
    public bool Success { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTime FirstGameDate { get; init; }
    public DateTime LastGameDate { get; init; }
};
