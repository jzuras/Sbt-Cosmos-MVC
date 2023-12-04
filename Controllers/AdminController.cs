using Microsoft.AspNetCore.Mvc;
using Sbt.Models.ViewModels;
using Sbt.Services;

namespace Sbt.Controllers.Admin;

 public class AdminController : Controller
{
    private readonly DivisionService _service;

    private DivisionService Service => _service;
    
    public List<string> OrganizationList;

    public AdminController(IConfiguration configuration, DivisionService service)
    {
        // this populates the HTML Select element list on the page.
        // The data should really come from the database, but I used this method
        // to learn about Configuration, as well as for simplicty.
        this.OrganizationList =
            configuration.GetSection("Organizations")?.Get<List<string>>() ?? new List<string>();

        this._service = service;
    }

    // GET: Index
    public IActionResult Index(string? organization)
    {
        return View(this.OrganizationList);
    }

    // GET: LoadSchedule
    public IActionResult LoadSchedule(string organization)
    {
        LoadScheduleViewModel model = new();

        return View(model);
    }

    // POST: LoadSchedule
    [HttpPost, ActionName("LoadSchedule")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoadSchedulePost(string organization, LoadScheduleViewModel model)
    {

        // submit button should be disbled if true, but protect against other entries
        if (SetCurrentOrganizationActionFilter.DisableSubmitButton == true)
        {
            return View(model);
        }

        if (organization == null || model.ScheduleFile == null || model.ScheduleFile.Length == 0)
        {
            return View(model);
        }

        var loadResult = await this.Service
            .LoadScheduleFileAsync(model.ScheduleFile, organization, model.DivisionID, model.UsesDoubleHeaders);

        if (loadResult.Success)
        {
            model.ResultMessage = DateTime.Now.ToShortTimeString() + ": Success loading schedule from " +
                model.ScheduleFile.FileName + ". <br>Games start on " +
                loadResult.FirstGameDate.ToShortDateString() +
                " and end on " +
                loadResult.LastGameDate.ToShortDateString();
        }
        else
        {
            model.ResultMessage = loadResult.ErrorMessage;
        }

        model.ResultSuccess = loadResult.Success;

        return View(model);
    }
}
