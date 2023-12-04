using Microsoft.AspNetCore.Mvc;
using Sbt.Models.ViewModels;
using Sbt.Services;

namespace Sbt.Controllers;

public class ScoresController : Controller
{
    private readonly DivisionService _service;

    private DivisionService Service => _service;

    public ScoresController(DivisionService service)
    {
        _service = service;
    }

    // GET: Scores/{organization}/{id}{gameID}
    public async Task<IActionResult> Index(string organization, string id, int gameID)
    {
        if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(id) || this.Service == null)
        {
            return NotFound();
        }

        var gameInfo = await this.Service.GetGames(organization, id, gameID);

        if (gameInfo == null)
        {
            return NotFound();
        }

        ScoresViewModel model = new();

        model.ID = id;
        model.Schedule = gameInfo;

        // populate ViewModel
        model.ScheduleVM = new List<Sbt.Models.ScheduleVM>();
        for (int i = 0; i < model.Schedule.Count; i++)
        {
            var scheduleVM = new Sbt.Models.ScheduleVM();
            scheduleVM.GameID = model.Schedule[i].GameID;
            scheduleVM.HomeScore = model.Schedule[i].HomeScore;
            scheduleVM.VisitorScore = model.Schedule[i].VisitorScore;
            scheduleVM.HomeForfeit = model.Schedule[i].HomeForfeit;
            scheduleVM.VisitorForfeit = model.Schedule[i].VisitorForfeit;
            model.ScheduleVM.Add(scheduleVM);
        }

        return View(model);
    }

    // POST: Scores/{organization}/{id}?gameID
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ScoresViewModel viewModel, string organization, string id)
    {
        if (!ModelState.IsValid || this.Service == null || 
            string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(id))
        {
                return NotFound();
        }

        await this.Service.SaveScores(organization, id, viewModel.ScheduleVM );

        return RedirectToAction(nameof(Index), "Standings", new { organization = organization, id = id });
    }
}