using Sbt.Data.Repositories;
using Sbt.Models;
using Sbt.Shared;

namespace Sbt.Services;

public class DivisionService
{
    private readonly IDivisionRepository _repository;

    public IDivisionRepository Repository => _repository;

    public DivisionService(IDivisionRepository divisionRepository)
    {
        this._repository = divisionRepository;
    }

    public async Task CreateDivisionInfo(DivisionInfo divisionInfo)
    {
        await this.Repository.SaveDivisionInfo(divisionInfo, false, true);
    }

    public async Task<LoadScheduleResult> LoadScheduleFileAsync(IFormFile scheduleFile, string organization, string divisionID,
        bool usesDoubleHeaders)
    {
        return await this.Repository.LoadScheduleFileAsync(scheduleFile, organization, divisionID, usesDoubleHeaders);
    }

    public async Task<List<DivisionInfo>> GetDivisionList(string organization)
    {
        return await this.Repository.GetDivisionList(organization);
    }

    public async Task<DivisionInfo?> GetDivisionInfoIfExists(string organization, string divisionID)
    {
        return await this.Repository.GetDivisionInfoIfExists(organization, divisionID.ToLower());
    }

    public async Task DeleteDivision(string organization, string divisionID)
    {
        var divisionToDelete = await this.Repository.GetDivisionInfoIfExists(organization, divisionID.ToLower());

        if (divisionToDelete == null)
        {
            throw new Sbt.Shared.Exceptions.DivisionNotFoundException();
        }
        await this.Repository.SaveDivisionInfo(divisionToDelete, true, false);
    }

    public async Task<Division> GetDivision(string organization, string divisionID)
    {
        return await this.Repository.GetDivision(organization, divisionID.ToLower());
    }

    public async Task<List<Schedule>> GetGames(string organization, string divisionID, int gameID)
    {
        return await this.Repository.GetGames(organization, divisionID.ToLower(), gameID);
    }

    public async Task SaveDivisionInfo(DivisionInfo divisionInfo)
    {
        await this.Repository.SaveDivisionInfo(divisionInfo, false, false);
    }
    public async Task SaveScores(string organization, string divisionID, IList<ScheduleVM> schedules)
    {
        await this.Repository.SaveScores(organization, divisionID.ToLower(), schedules);
    }
}
