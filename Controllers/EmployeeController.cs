using EmployeePortal.Models.Domain;
using EmployeePortal.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePortal.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeController(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        // GET: Dashboard (Staff Directory)
        public IActionResult Index(string searchName, int? deptId, int? typeId, int page = 1, int itemsPerPage = 5)
        {
            // 1. SECURITY CHECK: If the "AdminName" session is missing, redirect to Login
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminName")))
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Fetch data for dropdowns
            ViewBag.Departments = _repo.GetDepartments();
            ViewBag.EmployeeTypes = _repo.GetEmployeeTypes();

            // 3. Logic for filtering and pagination
            int totalRecords = _repo.GetCount(searchName, deptId, typeId);
            var employees = _repo.GetFiltered(searchName, deptId, typeId, page, itemsPerPage);

            // 4. Set state for the View
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = itemsPerPage;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / itemsPerPage);

            ViewData["CurrentFilter"] = searchName;
            ViewData["SelectedDept"] = deptId;
            ViewData["SelectedType"] = typeId;

            return View(employees);
        }
        // GET: Create Form
        public IActionResult Create()
        {
            ViewBag.Departments = _repo.GetDepartments();
            ViewBag.EmployeeTypes = _repo.GetEmployeeTypes();
            return View();
        }
        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            // 1. Fetch Admin from DB by Username
            // 2. Verify Hashed Password
            // 3. If valid, set Authentication Cookie or Session
            // 4. Redirect to Index
            return RedirectToAction("Index", "Employee");
        }
        // POST: Save New Employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Catch the actual ID returned by the database
                    int actualId = _repo.Add(employee);
                    employee.EmployeeId = actualId;

                    // Load names specifically for the Success summary page
                    ViewBag.DepartmentName = _repo.GetDepartments()
                        .FirstOrDefault(d => d.Id == employee.DepartmentId)?.Name;
                    ViewBag.TypeName = _repo.GetEmployeeTypes()
                        .FirstOrDefault(t => t.Id == employee.EmployeeTypeId)?.Name;

                    // Pointing to your 'Success.cshtml' view
                    return View("Success", employee);
                }
                catch (Microsoft.Data.SqlClient.SqlException ex)
                {
                    // Handle Duplicate Email Constraint
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        ModelState.AddModelError("Email", "This email address is already registered to another employee.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "A database error occurred. Please try again.");
                    }
                }
            }

            ViewBag.Departments = _repo.GetDepartments();
            ViewBag.EmployeeTypes = _repo.GetEmployeeTypes();
            return View(employee);
        }

        // GET: Success Summary Page
        public IActionResult Success(Employee employee)
        {
            var departments = _repo.GetDepartments();
            var types = _repo.GetEmployeeTypes();

            ViewBag.DepartmentName = departments.FirstOrDefault(d => d.Id == employee.DepartmentId)?.Name;
            ViewBag.TypeName = types.FirstOrDefault(t => t.Id == employee.EmployeeTypeId)?.Name;

            return View(employee);
        }

        // GET: View Details
        public IActionResult Details(int id)
        {
            var employee = _repo.GetById(id);
            if (employee == null) return NotFound();

            // Populate ViewBags for lookup resolution in the classy view
            ViewBag.Departments = _repo.GetDepartments();
            ViewBag.EmployeeTypes = _repo.GetEmployeeTypes();

            return View(employee);
        }

        // GET: Edit Form
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee = _repo.GetById(id);
            if (employee == null) return NotFound();

            ViewBag.Departments = _repo.GetDepartments();
            ViewBag.EmployeeTypes = _repo.GetEmployeeTypes();

            return View(employee);
        }

        // POST: Update Employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _repo.Update(employee);
                TempData["SuccessMessage"] = $"The profile for {employee.FullName} has been successfully updated.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = _repo.GetDepartments();
            ViewBag.EmployeeTypes = _repo.GetEmployeeTypes();
            return View(employee);
        }

        // GET: Delete Confirmation
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var employee = _repo.GetById(id);
            if (employee == null) return NotFound();

            ViewBag.Departments = _repo.GetDepartments();
            ViewBag.EmployeeTypes = _repo.GetEmployeeTypes();

            return View(employee);
        }

        // POST: Execute Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int employeeId)
        {
            _repo.Delete(employeeId);
            TempData["SuccessMessage"] = "Employee record has been securely terminated and removed from the directory.";
            return RedirectToAction(nameof(Index));
        }
    }
}