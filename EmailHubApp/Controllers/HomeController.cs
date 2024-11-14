using EmailHubApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EmailHubApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            #region Login Check
            if (HttpContext.Session.GetString("UserID") == "" || HttpContext.Session.GetString("UserID") == null)
                return RedirectToAction("Login", "User");
            #endregion
            return RedirectToAction("Index", "User");
        }     
    }
}