using EmployeePortal.Models;
using EmployeePortal.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EmployeePortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _repo;

        // Inject the same repository you used for the EmployeeController
        public HomeController(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            // Get the count without filters to show the total on the home page
            ViewBag.TotalRecords = _repo.GetCount(null, null, null);
            return View();
        }
    }
}