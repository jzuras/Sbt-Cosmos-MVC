using Sbt.Models;
using Sbt.Shared;

namespace Sbt.Data.Repositories;

public interface IDivisionRepository
{
    public Task<Division> GetDivision(string organization, string divisionID);

    public Task<DivisionInfo?> GetDivisionInfoIfExists(string organization, string divisionID);

    public Task<List<DivisionInfo>> GetDivisionList(string organization);

    public Task<List<Schedule>> GetGames(string organization, string divisionID, int gameID);

    public Task<LoadScheduleResult> LoadScheduleFileAsync(IFormFile scheduleFile, string organization, string divisionID,
        bool usesDoubleHeaders);

    public Task SaveDivisionInfo(DivisionInfo divisionInfo, bool deleteDivision = false, bool createDivision = false);

    public Task SaveScores(string organization, string divisionID, IList<ScheduleVM> schedules);
}
