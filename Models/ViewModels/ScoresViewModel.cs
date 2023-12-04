 namespace Sbt.Models.ViewModels;

public class ScoresViewModel
{
    // Division ID
    public string ID { get; set; } = default!;

    public IList<Sbt.Models.Schedule> Schedule { get; set; } = default!;

    public IList<Sbt.Models.ScheduleVM> ScheduleVM { get; set; } = default!;
}
