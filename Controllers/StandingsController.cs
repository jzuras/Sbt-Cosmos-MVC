using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Sbt.Models;
using Sbt.Models.ViewModels;
using Sbt.Services;

namespace Sbt.Controllers;

public class StandingsController : Controller
{
    private readonly DivisionService _service;

    private DivisionService Service => _service;

    public StandingsController(DivisionService service)
    {
        _service = service;
    }

    // GET: Standings/{organization}/{id}/{teamName?}
    // This method also handles partial rendering for AJAX
    public async Task<IActionResult> Index(string organization, string id, string? teamName)
    {
        if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(id) || this.Service == null)
        {
            return NotFound();
        }

        var divisionInfo = await Service.GetDivisionInfoIfExists(organization, id);
        if (divisionInfo == null)
        {
            return NotFound();
        }

        var division = await Service.GetDivision(organization, id);

        StandingsViewModel model = new();
        if (division == null || division.Organization == string.Empty)
        {
            model.DivisionInfo = divisionInfo;

            model.Schedule = new List<Schedule>();
            model.Standings = new List<Standings>();
            model.ShowOvertimeLosses = organization.ToLower().Contains("hockey");

            return View(model);
        }

        model.DivisionInfo = divisionInfo;

        model.Schedule = division.Schedule.ToList();
        model.Standings = division.Standings.ToList();

        if (!string.IsNullOrEmpty(teamName))
        {
            bool teamExists = model.Standings.Any(standing => standing.Name == teamName);
            if (teamExists)
            {
                model.TeamName = teamName;
                model.Schedule = model.Schedule
                    .Where<Schedule>(s => s.Home.ToLower() == teamName.ToLower() ||
                                     s.Visitor.ToLower() == teamName.ToLower()).ToList();
            }
        }

        model.Standings = model.Standings
            .OrderBy(s => s.GB).ThenByDescending(s => s.Percentage)
            .ToList();

        // in a production system this would be handled more generically,
        // but for now we are just checking if Org contains "Hockey"
        model.ShowOvertimeLosses = division.Organization.ToLower().Contains("hockey");

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            // The "_SchedulePartial" view inlcudes only the schedule portion
            // but we still need to call PartialView to avoid rendering _Layout.cshtml
            return PartialView("_SchedulePartial", model);
        }

        return View(model);
    }
}
