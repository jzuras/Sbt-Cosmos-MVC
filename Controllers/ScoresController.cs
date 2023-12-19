using Microsoft.AspNetCore.Mvc;
using Sbt.Models.Requests;
using Sbt.Models.ViewModels;
using Sbt.Services;

namespace Sbt.Controllers;

public class ScoresController : Controller
{
    private readonly DivisionService Service;

    public ScoresController(DivisionService service)
    {
        this.Service = service;
    }

    // GET: Scores/{organization}/{abbreviation}{gameID}
    [ParametersNotNullActionFilter(checkAbbreviation: false, checkDisableSubmitButton: false)]
    public async Task<IActionResult> Index(string organization, string abbreviation, int gameID)
    {
        try
        {
            var request = new GetScoresRequest
            {
                Organization = organization,
                Abbreviation = abbreviation,
                GameID = gameID
            };

            var response = await this.Service.GetGames(request);

            if(response.Success == false || response.Games == null)
            {
                return NotFound();
            }

            var games = response.Games;

            ScoresViewModel model = new(games, organization, abbreviation);

            return View(model);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // POST: Scores/{organization}/{abbreviation}?gameID
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ParametersNotNullActionFilter(checkAbbreviation: true, checkDisableSubmitButton: true)]
    public async Task<IActionResult> Index(ScoresViewModel model, string organization, string abbreviation)
    {
        // check for consistency errors within model
        if (!model.IsValid(out string errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // converting to a Request Object prevents overposting
        var request = model.ToScoresRequest();
        var response = await this.Service.SaveScores(request);

        if (!response.Success)
        {
            ModelState.AddModelError("", response.Message ?? "Unknown error.");
            return View(model);
        }

        return RedirectToAction(nameof(Index), "Standings", new { organization = organization, abbreviation = abbreviation });
    }
}