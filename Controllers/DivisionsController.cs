using Microsoft.AspNetCore.Mvc;
using Sbt.Models;
using Sbt.Models.ViewModels;
using Sbt.Services;
using Sbt.Shared.Exceptions;

namespace Sbt.Controllers;

public class DivisionsController : Controller
{
    private readonly DivisionService _service;

    private DivisionService Service => _service;

    public DivisionsController(DivisionService service)
    {
        _service = service;
    }

    #region Get Action Methods
    // GET: Divisions/Create/{organization}
    public IActionResult Create(string organization)
    {
        if (string.IsNullOrEmpty(organization) || this.Service == null)
        {
            return NotFound();
        }

        return View();
    }

    // GET: Divisions/Delete/{organization}/{id}
    public async Task<IActionResult> Delete(string organization, string id)
    {
        return await this.PopulateModel(organization, id);
    }

    // GET: Divisions/Details/{organization}/{id}
    public async Task<IActionResult> Details(string organization, string id)
    {
        return await this.PopulateModel(organization, id);
    }

    // GET: Divisions/Edit/{organization}/{id}
    public async Task<IActionResult> Edit(string organization, string id)
    {
        return await this.PopulateModel(organization, id);
    }

    // GET: Divisions/{organization}
    public async Task<IActionResult> Index(string organization)
    {
        if (string.IsNullOrEmpty(organization) || this.Service == null)
        {
            return NotFound();
        }

        var divisionsList = await Service.GetDivisionList(organization);

        DivisionListViewModel model = new();
        model.DivisionsList = divisionsList;

        return View(model);
    }
    #endregion

    #region Post Action Method
    // POST: Divisions/Create/{organization}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ID,League,NameOrNumber")] DivisionInfo model, 
        string organization)
    {
        // submit button should be disbled if true, but protect against other entries
        if (SetCurrentOrganizationActionFilter.DisableSubmitButton == true)
        {
            return View(model);
        }

        if (string.IsNullOrEmpty(organization) || this.Service == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                model.Organization = organization;
                model.ID = model.ID;
                await this.Service.CreateDivisionInfo(model);
            }
            catch (DivisionExistsException)
            {
                ModelState.AddModelError(string.Empty, "This Division ID already exists.");
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index), new { organization = organization });
    }

    // POST: Divisions/Edit/{organization}/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("ID,League,NameOrNumber,Locked")] DivisionInfo model,
        string organization, string id)
    {
        // submit button should be disbled if true, but protect against other entries
        if (SetCurrentOrganizationActionFilter.DisableSubmitButton == true)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(id) || this.Service == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            model.Organization = organization;
            model.ID = id;
            await this.Service.SaveDivisionInfo(model);
        }

        return RedirectToAction(nameof(Index), new { organization = organization });
    }

    // POST: Divisions/Delete/{organization}/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string organization, string id)
    {
        // submit button should be disbled if true, but protect against other entries
        if (SetCurrentOrganizationActionFilter.DisableSubmitButton == true)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(id) || this.Service == null)
        {
            return NotFound();
        }

        try
        {
            await this.Service.DeleteDivision(organization, id);

            return RedirectToAction(nameof(Index), new { organization = organization });
        }
        catch (DivisionNotFoundException)
        {
            // deleting a division that does not exist shouldn't happen
            return NotFound();
        }
        catch (Exception)
        {
            throw;
        }
    }
    #endregion

    #region Helper Method
    /// <summary>
    /// Common code used by Delete/Details/Edit Action Methods
    /// </summary>
    /// <param name="organization">Organization to search for.</param>
    /// <param name="id">ID of Division to search for.</param>
    /// <returns>View(model) if found, NotFound() otherwise.</returns>
    private async Task<IActionResult> PopulateModel(string organization, string id)
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
        
        return View(divisionInfo);
    }
    #endregion
}
