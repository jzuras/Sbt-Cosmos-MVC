using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sbt;

public class ParametersNotNullActionFilter : Attribute, IActionFilter
{
    private readonly bool CheckDisableSubmitButton;
    private readonly bool CheckAbbreviation;

    public ParametersNotNullActionFilter(bool checkAbbreviation = true, bool checkDisableSubmitButton = false)
    {
        this.CheckDisableSubmitButton = checkDisableSubmitButton;
        this.CheckAbbreviation = checkAbbreviation;
    }

    /// <summary>
    /// Intercept actions that request this to check for empty
    /// or null parameters. Also ensures no back-door entry on
    /// pages that use DisableSubmitButton flag.
    /// Forces a NotFound result if such issues are detected.
    /// Note - this filter runs second because global filters 
    /// run before action filters.
    /// </summary>
    /// <param name="context">Injected via DI.</param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // submit button should be disbled if true, but this protects against other entries
        if (this.CheckDisableSubmitButton && SetCurrentOrganizationActionFilter.DisableSubmitButton)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var organization = context.HttpContext.Request.RouteValues["organization"] as string;
        var abbreviation = context.HttpContext.Request.RouteValues["abbreviation"] as string;

        if (string.IsNullOrEmpty(organization) ||
            (this.CheckAbbreviation == true && string.IsNullOrEmpty(abbreviation)))
        {
            context.Result = new NotFoundResult();
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // required even if empty
    }
}

public class SetCurrentOrganizationActionFilter : IActionFilter
{
    // for now, this is a quick way to disable Admin Functions on Azure
    // (Global variables are a horrible idea - this needs to be a
    // Feature on Azure which can be turned on or off remotely.)
    // Alternatively, I may handle this with Identity and Autherization.
    public static bool DisableSubmitButton = true;

    private readonly string _viewDataOrgDefault = "Demo Softball";

    public SetCurrentOrganizationActionFilter()
    {
    }

    /// <summary>
    /// Intercept every action and pull the Organization from the Route,
    /// then set a static variable to be used by View Component.
    /// </summary>
    /// <param name="context">Injected via DI.</param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Get the organization parameter from the route or query string
        string? organization = context.RouteData.Values["organization"] as string;

        if (string.IsNullOrEmpty(organization))
        {
            organization = this._viewDataOrgDefault;
        }

        if (context.Controller is Controller controller)
        {
            controller.TempData.CurrentOrganization(organization);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // required even if empty
    }
}
