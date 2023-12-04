using Microsoft.AspNetCore.Mvc;
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

    // GET: Standings/{organization}/{id}
    public async Task<IActionResult> Index(string organization, string id)
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

        if (division == null || division.Organization == string.Empty)
        {
            return NotFound();
        }

        StandingsViewModel model = new();
        //model.Organization = organization;
        model.DivisionInfo = divisionInfo;

        model.Schedule = division.Schedule.ToList();
        model.Standings = division.Standings.ToList();

        if (Request.Query.TryGetValue("teamName", out var teamName))
        {
            model.TeamName = teamName;
            model.Schedule = model.Schedule
                .Where<Schedule>(s => s.Home == teamName || s.Visitor == teamName).ToList();
        }

        model.Standings = model.Standings
            .OrderBy(s => s.GB).ThenByDescending(s => s.Percentage)
            .ToList();

        // in a production system this would be handled more generically,
        // but for now we are just checking if Org contains "Hockey"
        model.ShowOvertimeLosses = division.Organization.ToLower().Contains("hockey");

        return View(model);
    }
}
