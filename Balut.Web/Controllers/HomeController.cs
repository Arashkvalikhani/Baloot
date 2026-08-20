using Microsoft.AspNetCore.Mvc;

namespace Balut.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            if (User.IsInRole("Teacher"))
                return RedirectToAction("Index", "TeacherPanel");

            if (User.IsInRole("Student"))
                return RedirectToAction("Index", "StudentPanel");

            if (User.IsInRole("Parent"))
                return RedirectToAction("Index", "ParentPanel");

            return RedirectToAction("Index", "Dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}