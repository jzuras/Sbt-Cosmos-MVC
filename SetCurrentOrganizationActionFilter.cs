using Microsoft.AspNetCore.Mvc.Filters;

namespace Sbt;

public class SetCurrentOrganizationActionFilter : IActionFilter
{
    // for now, this is a quick way to disable Admin Functions on Azure
    // (Global variables are a horrible idea - this needs to be a
    // Feature on Azure which can be turned on or off remotely.)
    public static bool DisableSubmitButton = false;

    // Each View references this to set a header.
    public static string CurrentOrganization = "Demo Softball";

    private readonly string _viewDataOrgDefault = "Demo Softball";


    /// <summary>
    /// Intercept every action and pull the Organization from the Route,
    /// then set a static variable to be used by Views.
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
        
        CurrentOrganization = organization;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // placeholder for future reference
    }
}
