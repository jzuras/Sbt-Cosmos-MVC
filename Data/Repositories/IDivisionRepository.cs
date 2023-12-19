using Sbt.Models;

namespace Sbt.Data.Repositories;

public interface IDivisionRepository
{
    public Task<bool> DivisionExists(string organization, string abbreviation);

    public Task<Division> GetDivision(string organization, string abbreviation);

    public Task<List<Division>> GetDivisionList(string organization);

    public Task<List<Schedule>> GetGames(string organization, string abbreviation, int gameID);

    public Task SaveDivision(Division division, bool deleteDivision = false, bool createDivision = false);
}
