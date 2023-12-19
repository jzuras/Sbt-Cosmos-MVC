using Microsoft.AspNetCore.Mvc;
using Sbt.Models.Requests;
using Sbt.Services;

namespace Sbt.Controllers;

public class StandingsListController : Controller
{
    private readonly DivisionService Service;

    public StandingsListController(DivisionService service)
    {
        this.Service = service;
    }

    // GET: StandingsList/{organization}
    [ParametersNotNullActionFilter(checkAbbreviation: false, checkDisableSubmitButton: false)]
    public async Task<IActionResult> Index(string organization)
    {
        var request = new GetDivisionListRequest
        {
            Organization = organization,
        };

        var response = await this.Service.GetDivisionList(request);

        if (response.Success == false || response.DivisionList == null)
        {
            return NotFound();
        }

        return View(response.DivisionList);
    }

}
