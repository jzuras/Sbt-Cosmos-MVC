using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Sbt.Models;
using System.Net;

namespace Sbt.Data.Repositories;

public class DivisionEfCoreRepository : IDivisionRepository
{
    private readonly DivisionContext _dbContext;

    public DivisionContext DbContext => _dbContext;

    public DivisionEfCoreRepository(DivisionContext context)
    {
        this._dbContext = context;
    }

    public async Task<bool> DivisionExists(string organization, string abbreviation)
    {
        var division = await this.GetDivision(organization, abbreviation);

        return (division != null);
    }

    public async Task<Division> GetDivision(string organization, string abbreviation)
    {
        try
        {
            var division = await this.DbContext.Divisions
                .WithPartitionKey(organization)
                .Where(d => d.Organization.ToLower() == organization.ToLower()
                    && d.Abbreviation.ToLower() == abbreviation.ToLower())
                .FirstOrDefaultAsync();

            return division!;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<Division>> GetDivisionList(string organization)
    {
        try
        {
            var division = await this.DbContext.Divisions
                .WithPartitionKey(organization)
                .Where(d => d.Organization.ToLower() == organization.ToLower()).ToListAsync();

            if (division != null)
                return division;
            else
                return new List<Division>();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // No divisions - return empty list
            return new List<Division>();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<Schedule>> GetGames(string organization, string abbreviation, int gameID)
    {
        var list = new List<Schedule>();

        try
        {
            // Step 1: do a query returning 1 game result based on the game id
            // Step 2: do a second query using that game's day and field
            //var schedule = await this.Divisions
            //    .Where(d => d.Organization == organization && d.ID == divisionID.ToLower())
            //    .SelectMany(d => d.Schedule)
            //    .Where(s => s.GameID == gameID)
            //    .FirstOrDefaultAsync();

            // ef core could not handle the query above (threw exception about inability to translate)
            // so now we just get the entire division and query the schedule list directly

            var division = await this.GetDivision(organization, abbreviation);

            var games = division.Schedule
                .Where(s => s.GameID == gameID)
                .SelectMany(s => division.Schedule.Where(inner => inner.Day == s.Day && inner.Field == s.Field))
                .ToList();

            if (games != null)
            {
                foreach (var game in games)
                {
                    list.Add(game);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }

        return list;
    }

    /// <summary>
    /// Handles creating, deleting, and updating divisions. 
    /// Calling method is expected to enforce requirements such as 
    /// not creating a divsiion that already exists, if so desired.
    /// </summary>
    /// <param name="division">Division object which is being tracked, except for creating.</param>
    /// <param name="deleteDivision">True to delete this division, false otherwise.</param>
    /// <param name="createDivision">True to create this division, false otherwise.</param>
    public async Task SaveDivision(Division division,
                                   bool deleteDivision = false, bool createDivision = false)
    {
        try
        {
            if (deleteDivision)
            {
                this.DbContext.Divisions.Remove(division);
            }
            else if (createDivision)
            {
                this.DbContext.Divisions.Add(division);
            }
            else
            {
                this.DbContext.Update(division);
            }

            await this.DbContext.SaveChangesAsync();
            return;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
