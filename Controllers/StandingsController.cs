using Microsoft.AspNetCore.Mvc;
using Sbt.Models;
using Sbt.Models.Requests;
using Sbt.Models.ViewModels;
using Sbt.Services;

namespace Sbt.Controllers;

public class StandingsController : Controller
{
    private readonly DivisionService Service;

    public StandingsController(DivisionService service)
    {
        this.Service = service;
    }

    // GET: Standings/{organization}/{abbreviation}/{teamName?}
    // This method also handles partial rendering for AJAX
    [ParametersNotNullActionFilter(checkAbbreviation: true, checkDisableSubmitButton: false)]
    public async Task<IActionResult> Index(string organization, string abbreviation, string? teamName)
    {
        var request = new GetDivisionRequest
        {
            Organization = organization,
            Abbreviation = abbreviation,
        };

        var response = await this.Service.GetDivision(request);

        if (response.Success == false || response.Division == null)
        {
            return NotFound();
        }

        var division = response.Division;

        StandingsViewModel model = new();
        if (division == null || division.Organization == string.Empty)
        {
            // render empty schedule and standings
            model.Division.Schedule = new List<Schedule>();
            model.Division.Standings = new List<Standings>();
            model.ShowOvertimeLosses = organization.ToLower().Contains("hockey");

            return View(model);
        }

        model.Division = division;
        model.Division.Standings = model.Division.Standings
            .OrderBy(s => s.GB).ThenByDescending(s => s.Percentage).ToList();

        if (!string.IsNullOrEmpty(teamName))
        {
            bool teamExists = model.Division.Standings.Any(standing => standing.Name.ToLower() == teamName.ToLower());
            if (teamExists)
            {
                model.TeamName = teamName;
                model.Division.Schedule = model.Division.Schedule
                    .Where<Schedule>(s => s.Home.ToLower() == teamName.ToLower() ||
                                     s.Visitor.ToLower() == teamName.ToLower()).ToList();
            }
        }

        model.ShowOvertimeLosses = division.Organization.ToLower().Contains("hockey");

        if (base.Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            // The "_SchedulePartial" view inlcudes only the schedule portion
            // but we still need to call PartialView to avoid rendering _Layout.cshtml
            return PartialView("_SchedulePartial", model);
        }

        return View(model);
    }
}
