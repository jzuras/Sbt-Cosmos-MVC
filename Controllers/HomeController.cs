using Microsoft.AspNetCore.Mvc;
using Sbt.Models.ViewModels;
using System.Diagnostics;

namespace Sbt.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> Logger;

    public HomeController(ILogger<HomeController> logger)
    {
        this.Logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
