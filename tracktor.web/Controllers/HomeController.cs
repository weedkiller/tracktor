using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace tracktor.web.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Title = "tracktor";
            ViewBag.User = Request.GetOwinContext().Authentication.User.Identity.Name;

            return View();
        }

        public ActionResult SignIn()
        {
            ViewBag.Title = "sign in";

            return View();
        }
    }
}
