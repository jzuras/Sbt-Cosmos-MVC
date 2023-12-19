using Microsoft.AspNetCore.Mvc;
using Sbt.Models.Requests;
using Sbt.Models.ViewModels;
using Sbt.Services;

namespace Sbt.Controllers.Admin;

 public class AdminController : Controller
{
    private readonly DivisionService Service;
    
    public List<string> OrganizationList;

    public AdminController(IConfiguration configuration, DivisionService service)
    {
        // this populates the HTML Select element list on the page.
        // The data should really come from the database, but I used this method
        // to learn about Configuration, as well as for simplicty.
        this.OrganizationList =
            configuration.GetSection("Organizations")?.Get<List<string>>() ?? new List<string>();

        this.Service = service;
    }

    [AcceptVerbs("Get", "Post")]
    [Route("/Admin/DivisionExists")]
    public async Task<IActionResult> DivisionExists(string organization, string abbreviation)
    {
        // this action method handles the Remote attribute on an Abbreviation input,
        // and will return false if the division can be found, true if not.

        var request = new DivisionExistsRequest
        {
            Organization = organization,
            Abbreviation = abbreviation,
        };

        var response = await this.Service.DivisionExists(request);

        // if the division is found, a schedule can then be loaded for it.
        bool divisionFound = response.Success;

        return Json(divisionFound);
    }

    // GET: Admin/{organization?}
    public IActionResult Index(string? organization)
    {
        var model = (organization, this.OrganizationList);
        return View(model);
    }

    // GET: Admin/LoadSchedule/{organization}
    [ParametersNotNullActionFilter(checkAbbreviation: false, checkDisableSubmitButton: false)]
    public IActionResult LoadSchedule(string organization)
    {
        LoadScheduleViewModel model = new();

        model.Organization = organization;

        return View(model);
    }

    // POST: Admin/LoadSchedule/{organization}
    [ParametersNotNullActionFilter(checkAbbreviation: false, checkDisableSubmitButton: true)]
    [HttpPost, ActionName("LoadSchedule")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoadSchedulePost(string organization, LoadScheduleViewModel model)
    {
        var request = new LoadScheduleRequest
        {
            Organization = organization,
            Abbreviation = model.Abbreviation,
            ScheduleFile = model.ScheduleFile,
            UsesDoubleHeaders = model.UsesDoubleHeaders,
        };

        var response = await this.Service.LoadScheduleFileAsync(request);

        if (response.Success)
        {
            model.ResultMessage = DateTime.Now.ToShortTimeString() + ": Success loading schedule from " +
                model.ScheduleFile.FileName + ". <br>Games start on " +
                response.FirstGameDate.ToShortDateString() +
                " and end on " +
                response.LastGameDate.ToShortDateString();
        }
        else
        {
            model.ResultMessage = DateTime.Now.ToShortTimeString() + ": Failure loading schedule from " +
                model.ScheduleFile.FileName + ". <br>Error message: " + response.Message;
        }

        model.ResultSuccess = response.Success;

        return View(model);
    }
}
