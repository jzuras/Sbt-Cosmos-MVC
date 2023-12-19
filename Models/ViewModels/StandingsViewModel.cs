namespace Sbt.Models.ViewModels;

public class StandingsViewModel
{
    public Sbt.Models.Division Division { get; set; } = default!;

    public bool ShowOvertimeLosses { get; set; } = false;

    public string? TeamName { get; set; }

}
