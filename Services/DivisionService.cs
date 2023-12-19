using Sbt.Data.Repositories;
using Sbt.Models;
using Sbt.Models.Requests;

namespace Sbt.Services;

public class DivisionService
{
    private readonly IDivisionRepository _repository;

    public IDivisionRepository Repository => _repository;

    public DivisionService(IDivisionRepository divisionRepository)
    {
        this._repository = divisionRepository;
    }

    public async Task<DivisionExistsResponse> DivisionExists(DivisionExistsRequest request)
    {
        try
        {
            var exists = await this.Repository.DivisionExists(request.Organization, request.Abbreviation);

            return new DivisionExistsResponse
            {
                Success = exists,
            };
        }
        catch (Exception ex)
        {
            return new DivisionExistsResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    public async Task<CreateDivisionResponse> CreateDivision(CreateDivisionRequest request)
    {
        try
        {
            var exists = await this.Repository.DivisionExists(request.Organization, request.Abbreviation);

            if (!exists)
            {
                var division = new Division
                {
                    Organization = request.Organization,
                    Abbreviation = request.Abbreviation,
                    League = request.League,
                    NameOrNumber = request.NameOrNumber,
                    Updated = this.GetEasternTime(),
                };

                await this.Repository.SaveDivision(division, false, true);

                return new CreateDivisionResponse
                {
                    Success = true,
                };
            }
            else
            {
                return new CreateDivisionResponse
                {
                    Success = false,
                    Message = "Unable to create division because a division already exists with this Abbreviation."
                };
            }
        }
        catch (Exception ex)
        {
            return new CreateDivisionResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    public async Task<LoadScheduleResponse> LoadScheduleFileAsync(LoadScheduleRequest request)
    {
        try
        {
            var divisionExists = await this.Repository.DivisionExists(request.Organization, request.Abbreviation);

            if (divisionExists)
            {
                using (var stream = request.ScheduleFile.OpenReadStream())
                {
                    var (firstGameDate, lastGameDate) = await this.LoadScheduleFileAsync(stream,
                        request.Organization, request.Abbreviation, request.UsesDoubleHeaders);

                    return new LoadScheduleResponse
                    {
                        Success = true,
                        FirstGameDate = firstGameDate,
                        LastGameDate = lastGameDate
                    };
                }
            }
            else
            {
                return new LoadScheduleResponse
                {
                    Success = false,
                    Message = "Unable to load schedule - division not found."
                };
            }
        }
        catch (Exception ex)
        {
            return new LoadScheduleResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    public async Task<GetDivisionListResponse> GetDivisionList(GetDivisionListRequest request)
    {
        try
        {
            var list = await this.Repository.GetDivisionList(request.Organization);

            return new GetDivisionListResponse
            {
                Success = true,
                DivisionList = list
            };
        }
        catch (Exception ex)
        {
            return new GetDivisionListResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    public async Task<DeleteDivisionResponse> DeleteDivision(DeleteDivisionRequest request)
    {
        try
        {
            var division = await this.Repository.GetDivision(request.Organization, request.Abbreviation);

            if (division == null)
            {
                return new DeleteDivisionResponse
                {
                    Success = false,
                    Message = "Unable to delete division because no division exists with this Abbreviation."
                };
            }
            
            await this.Repository.SaveDivision(division, true, false);

            return new DeleteDivisionResponse
            {
                Success = true,
            };
        }
        catch (Exception ex)
        {
            return new DeleteDivisionResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    public async Task<GetDivisionResponse> GetDivision(GetDivisionRequest request)
    {
        try
        {
            var division = await this.GetDivision(request.Organization, request.Abbreviation);

            return new GetDivisionResponse
            {
                Success = true,
                Division = division
            };
        }
        catch (Exception ex)
        {
            return new GetDivisionResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    public async Task<GetScoresResponse> GetGames(GetScoresRequest request)
    {
        try
        {
            var games = await this.Repository.GetGames(
                request.Organization, request.Abbreviation, request.GameID);

            return new GetScoresResponse
            {
                Success = true,
                Games = games
            };
        }
        catch (Exception ex)
        {
            return new GetScoresResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    public async Task<UpdateDivisionResponse> UpdateDivision(UpdateDivisionRequest request)
    {
        try
        {
            var division = await this.Repository.GetDivision(request.Organization, request.Abbreviation);

            if (division != null)
            {
                division.League = request.League;
                division.NameOrNumber = request.NameOrNumber;
                division.Locked = request.Locked;
                division.Updated = this.GetEasternTime();

                await this.Repository.SaveDivision(division, false, false);

                return new UpdateDivisionResponse
                {
                    Success = true,
                };
            }
            else
            {
                return new UpdateDivisionResponse
                {
                    Success = false,
                    Message = "Unable to update division because no division exists with this Abbreviation."
                };
            }
        }
        catch (Exception ex)
        {
            return new UpdateDivisionResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    public async Task<UpdateScoresResponse> SaveScores(UpdateScoresRequest request)
    {
        try
        {
            var division = await this.Repository.GetDivision(request.Organization, request.Abbreviation);

            if (division == null)
            {
                return new UpdateScoresResponse
                {
                    Success = false,
                    Message = "Unable to save scores because no division exists with this Abbreviation."
                };
            }

            this.ProcessScores(division, request.Scores);
            division.Updated = this.GetEasternTime();
            await this.Repository.SaveDivision(division, false, false);

            return new UpdateScoresResponse
            {
                Success = true,
                Message = $"Successfully updated \"{request.Abbreviation}\"",
            };
        }
        catch (Exception ex)
        {
            return new UpdateScoresResponse
            {
                Success = false,
                Message = ex.Message,
            };
        }
    }

    #region Helper Methods
    private async Task<Division> GetDivision(string organization, string abbreviation)
    {
        return await this.Repository.GetDivision(organization, abbreviation);
    }

    #region Processing Methods
    private void ProcessScores(Division division, IList<ScheduleSubsetForUpdateScoresRequest> scores)
    {
        try
        {
            for (int i = 0; i < scores.Count; i++)
            {
                // find matching game id
                var gameToUpdate = division.Schedule.FirstOrDefault(s => s.GameID == scores[i].GameID);

                if (gameToUpdate != null)
                {
                    gameToUpdate.HomeForfeit = scores[i].HomeForfeit;
                    gameToUpdate.HomeScore = scores[i].HomeScore;
                    gameToUpdate.VisitorForfeit = scores[i].VisitorForfeit;
                    gameToUpdate.VisitorScore = scores[i].VisitorScore;
                }
            }

            this.ReCalcStandings(division);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<(DateTime StartTime, DateTime EndTime)> LoadScheduleFileAsync(Stream scheduleFileStream,
        string organization, string abbreviation, bool usesDoubleHeaders)
    {
        string errorMessage = string.Empty;
        Division? division;

        // out params for processing method:
        DateTime firstGameDate;
        DateTime lastGameDate;
        List<Standings> standings;
        List<Schedule> schedule;

        try
        {
            division = await this.GetDivision(organization, abbreviation);

            if (division == null)
            {
                throw new Exception("Unable to load schedule - division not found.");
            }
        }
        catch (Exception)
        {
            throw;
        }

        if (this.ProcessScheduleFile(scheduleFileStream, usesDoubleHeaders,
            out standings, out schedule, out firstGameDate, out lastGameDate, out errorMessage))
        {
            division.Schedule = schedule;
            division.Standings = standings;

            try
            {
                division.Updated = this.GetEasternTime();
                await this.Repository.SaveDivision(division, false, false);

            }
            catch (Exception)
            {
                throw;
            }
        }
        else
        {
            // processing failed
            throw new Exception(errorMessage);
        }

        return (firstGameDate, lastGameDate);
    }
    
    private void ReCalcStandings(Division division)
    {
        var standings = division.Standings;

        var schedule = division.Schedule;

        // zero-out standings
        foreach (var stand in standings)
        {
            stand.Forfeits = stand.Losses = stand.OvertimeLosses = stand.Ties = stand.Wins = 0;
            stand.RunsAgainst = stand.RunsScored = stand.ForfeitsCharged = 0;
            stand.GB = stand.Percentage = 0;
        }

        foreach (var sched in schedule)
        {
            // skip week boundary
            if (sched.Visitor.ToUpper().StartsWith("WEEK") == true) continue;

            this.UpdateStandings(standings, sched);
        }
    }

    private void UpdateStandings(List<Standings> standings, Schedule sched)
    {
        // note - IList starts at 0, team IDs start at 1
        var homeTteam = standings[sched.HomeID - 1];
        var visitorTeam = standings[sched.VisitorID - 1];

        if (sched.HomeScore > -1) // this will catch null values (no scores reported yet)
        {
            homeTteam.RunsScored += (short)sched.HomeScore!;
            homeTteam.RunsAgainst += (short)sched.VisitorScore!;
            visitorTeam.RunsScored += (short)sched.VisitorScore!;
            visitorTeam.RunsAgainst += (short)sched.HomeScore!;
        }

        if (sched.HomeForfeit)
        {
            homeTteam.Forfeits++;
            homeTteam.ForfeitsCharged++;
        }
        if (sched.VisitorForfeit)
        {
            visitorTeam.Forfeits++;
            visitorTeam.ForfeitsCharged++;
        }

        if (sched.VisitorForfeit && sched.HomeForfeit)
        {
            // special case - not a tie - counted as losses for both team
            homeTteam.Losses++;
            visitorTeam.Losses++;
        }
        else if (sched.HomeScore > sched.VisitorScore)
        {
            homeTteam.Wins++;
            visitorTeam.Losses++;
        }
        else if (sched.HomeScore < sched.VisitorScore)
        {
            homeTteam.Losses++;
            visitorTeam.Wins++;
        }
        else if (sched.HomeScore > -1) // this will catch null values (no scores reported yet)
        {
            homeTteam.Ties++;
            visitorTeam.Ties++;
        }

        // calculate Games Behind (GB)
        var sortedTeams = standings.OrderByDescending(t => t.Wins).ToList();
        var maxWins = sortedTeams.First().Wins;
        var maxLosses = sortedTeams.First().Losses;
        foreach (var team in sortedTeams)
        {
            team.GB = ((maxWins - team.Wins) + (team.Losses - maxLosses)) / 2.0f;
            if ((team.Wins + team.Losses) == 0)
            {
                team.Percentage = 0.0f;
            }
            else
            {
                team.Percentage = (float)team.Wins / (team.Wins + team.Losses + team.Ties);
            }
        }
    }

    private bool ProcessScheduleFile(Stream scheduleFileStream, bool usesDoubleHeaders,
        out List<Standings> standings, out List<Schedule> schedule,
        out DateTime firstGameDate, out DateTime lastGameDate,
        out string errorMessage)
    {
        int gameID = 0; // NOTE - Game IDs are unique within a Division (document) for Cosmos
        int lineNumber = 0;
        List<string> lines = new();

        standings = new List<Standings>();
        schedule = new List<Schedule>();
        firstGameDate = DateTime.MinValue;
        lastGameDate = DateTime.MinValue;

        using (var reader = new StreamReader(scheduleFileStream))
        {
            while (reader.Peek() >= 0)
                lines.Add(reader.ReadLine()!);
        }

        try
        {
            // Note - expecting a properly formatted file since it is self-created,
            // solely for the purposes of populating some demo data for the website.
            // therefore no error-checking is done here - just wrapping in try-catch
            // and returning exceptions to the calling method

            List<string> teams = new();
            short teamID = 1;

            // skip first 4 lines which are simply for ease of reading the file
            lineNumber = 4;

            // next lines are teams - ended by blank line
            // team IDs are assumed, starting at 1
            while (lines[lineNumber].Length > 0)
            {
                teams.Add(lines[lineNumber].Trim());

                // create standings row for each team
                var standingsRow = new Standings
                {
                    Wins = 0,
                    Losses = 0,
                    Ties = 0,
                    OvertimeLosses = 0,
                    Percentage = 0,
                    GB = 0,
                    RunsAgainst = 0,
                    RunsScored = 0,
                    Forfeits = 0,
                    ForfeitsCharged = 0,
                    Name = lines[lineNumber].Trim(),
                    TeamID = teamID++
                };
                standings.Add(standingsRow);
                lineNumber++;
            }

            // the rest of file is the actual schedule, in this format:
            // Date,Day,Time,Home,Visitor,Field
            for (int index = lineNumber + 1; index < lines.Count; index++)
            {
                string[] data = lines[index].Split(',');

                if (data[0].ToLower().StartsWith("week"))
                {
                    // original code had complicated method to determine week boundaries,
                    // but for simplicity's sake I am now manually adding this info in the schedule files
                    schedule.Add(this.AddWeekBoundary(data[0], gameID));
                    gameID++;
                    continue;
                }
                DateTime gameDate = DateTime.Parse(data[0]);
                // skipping value at [1] - not currently used in this version of the website
                DateTime gameTime = DateTime.Parse(data[2]);
                short homeTeamID = short.Parse(data[3]);
                short visitorTeamID = short.Parse(data[4]);
                string field = data[5];

                // create schedule row for each game
                var scheduleRow = new Schedule
                {
                    GameID = gameID++,
                    Day = gameDate,
                    Field = field,
                    Home = teams[homeTeamID - 1],
                    HomeForfeit = false,
                    HomeID = homeTeamID,
                    Time = gameTime,
                    Visitor = teams[visitorTeamID - 1],
                    VisitorForfeit = false,
                    VisitorID = visitorTeamID,
                };
                schedule.Add(scheduleRow);

                if (usesDoubleHeaders)
                {
                    // add a second game 90 minutes later, swapping home/visitor
                    scheduleRow = new Schedule
                    {
                        GameID = gameID++,
                        Day = gameDate,
                        Field = field,
                        Home = teams[visitorTeamID - 1],
                        HomeForfeit = false,
                        HomeID = visitorTeamID,
                        Time = gameTime.AddMinutes(90),
                        Visitor = teams[homeTeamID - 1],
                        VisitorForfeit = false,
                        VisitorID = homeTeamID,
                    };
                    schedule.Add(scheduleRow);
                }

                // keep track of first and last games so user can verify full schedule was loaded.
                if (index == lineNumber + 2)
                {
                    firstGameDate = gameDate;
                }
                else if (index == lines.Count - 1)
                {
                    lastGameDate = gameDate;
                }
            } // for loop processing schedule data
        }
        catch (Exception ex)
        {
            errorMessage = "Line number: " + lineNumber + " " + ex.Message;
            return false;
        }
        errorMessage = string.Empty;
        return true;
    }

    private DateTime GetEasternTime()
    {
        DateTime utcTime = DateTime.UtcNow;

        TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternTimeZone);
    }

    private Schedule AddWeekBoundary(string week, int maxGameID)
    {
        // this creates a mostly empty "WEEK #" row to make it easier to show
        // week boundaries when displaying the schedule.
        var scheduleRow = new Schedule
        {
            GameID = maxGameID,
            HomeForfeit = false,
            Visitor = week,
            VisitorForfeit = false,
        };

        return scheduleRow;
    }
    #endregion

    #endregion
}
