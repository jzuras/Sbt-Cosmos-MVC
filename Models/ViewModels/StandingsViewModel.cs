using Microsoft.AspNetCore.Mvc;

namespace Sbt.Models.ViewModels;

public class StandingsViewModel
{
    public IList<Sbt.Models.Standings> Standings { get; set; } = default!;

    public IList<Sbt.Models.Schedule> Schedule { get; set; } = default!;

    public Sbt.Models.DivisionInfo DivisionInfo { get; set; } = default!;

    public bool ShowOvertimeLosses { get; set; } = false;

    [BindProperty]
    public string? TeamName { get; set; }

}
