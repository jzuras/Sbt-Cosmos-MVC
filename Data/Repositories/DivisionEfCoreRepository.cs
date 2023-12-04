using Sbt.Models;
using Sbt.Shared;

namespace Sbt.Data.Repositories;

public class DivisionEfCoreRepository : IDivisionRepository
{
    private readonly DivisionContext _dbContext;

    public DivisionContext DbContext => _dbContext;

    public DivisionEfCoreRepository(DivisionContext context)
    {
        this._dbContext = context;
    }

    public async Task<Division> GetDivision(string organization, string divisionID)
    {
        return await this.DbContext.GetDivision(organization, divisionID);
    }

    public async Task<List<DivisionInfo>> GetDivisionList(string organization)
    {
        return await this.DbContext.GetDivisionList(organization);
    }

    public async Task<DivisionInfo?> GetDivisionInfoIfExists(string organization, string divisionID)
    {
        return await this.DbContext.GetDivisionInfoIfExists(organization, divisionID);
    }

    public async Task<List<Schedule>> GetGames(string organization, string divisionID, int gameID)
    {
        return await this.DbContext.GetGames(organization, divisionID, gameID);
    }

    public async Task<LoadScheduleResult> LoadScheduleFileAsync(IFormFile scheduleFile, string organization, string divisionID,
    bool usesDoubleHeaders)
    {
        return await this.DbContext.LoadScheduleFileAsync(scheduleFile, organization, divisionID, usesDoubleHeaders);
    }

    public async Task SaveDivisionInfo(DivisionInfo divisionInfo,
        bool deleteDivision = false, bool createDivision = false)
    {
        await this.DbContext.SaveDivisionInfo(divisionInfo, deleteDivision, createDivision);
    }

    public async Task SaveScores(string organization, string divisionID, IList<ScheduleVM> schedules)
    {
        await this.DbContext.SaveScores(organization, divisionID, schedules);
    }
}
