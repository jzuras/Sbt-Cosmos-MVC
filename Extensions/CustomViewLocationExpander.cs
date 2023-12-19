using Microsoft.AspNetCore.Mvc.Razor;

namespace Sbt;

public class CustomViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        // add values to the route values here if needed
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        // Add to the viewLocations list to search Divisions within Admin
        var additionalLocation = "/Views/Admin/Divisions/{0}.cshtml";

        return viewLocations.Concat(new[] { additionalLocation });
    }
}
