using Microsoft.AspNetCore.Mvc;
using Sbt.Models.ViewModels;
using Sbt.Services;

namespace Sbt.Controllers;

public class StandingsListController : Controller
{
    private readonly DivisionService _service;

    private DivisionService Service => _service;

    public StandingsListController(DivisionService service)
    {
        _service = service;
    }

    // GET: StandingsList/{organization}
    public async Task<IActionResult> Index(string organization)
    {
        if (string.IsNullOrEmpty(organization) || this.Service == null)
        {
            return NotFound();
        }

        var divisionsList = await Service.GetDivisionList(organization);

        DivisionListViewModel model = new();
        //model.Organization = organization;
        model.DivisionsList = divisionsList;

        return View(model);
    }

}
