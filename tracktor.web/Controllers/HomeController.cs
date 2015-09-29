// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using tracktor.service;

namespace tracktor.web.Controllers
{
    public class HomeController : Controller
    {
        private ITracktorService _service;

        public HomeController(ITracktorService service)
        {
            _service = service;
        }

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Title = "tracktor";
            ViewBag.User = Request.GetOwinContext().Authentication.User.Identity.Name;
            int userID;
            var timezone = TracktorController.GetUserTimezone(Request.GetOwinContext(), out userID);
            ViewBag.Timezone = timezone.Id;

            return View();
        }

        public ActionResult SignIn()
        {
            ViewBag.Title = "sign in";

            return View();
        }

        [Authorize]
        public FileResult CSV()
        {
            try
            {
                var context = TracktorController.GetContext(Request.GetOwinContext());
                var entries = _service.GetEntriesModel(context, null, null, 0, 0, 99999);

                var csvFile = new StringBuilder();
                csvFile.AppendLine("Start,End,Month,Project,Task,Hours,InProgress");
                foreach (var entry in entries.Entries)
                {
                    csvFile.AppendLine(
                        String.Format(
                            "{0},{1},{2},{3},{4},{5},{6}",
                            entry.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            entry.EndDate.HasValue ? entry.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                            entry.StartDate.ToString("yyyy-MMM"),
                            entry.ProjectName,
                            entry.TaskName,
                            (entry.Contrib / 3600).ToString(), // hours
                            entry.InProgress ? "Yes" : "No"));
                }
                return File(
                    System.Text.Encoding.UTF8.GetBytes(csvFile.ToString()),
                    "text/csv",
                    string.Format("tracktor tasks {0}.csv", DateTime.UtcNow.ToString("yyyy-MM-dd")));
            }
            catch (Exception ex)
            {
                return File(
                    System.Text.Encoding.UTF8.GetBytes("Error: " + ex.Message),
                    "text/plain",
                    string.Format("error {0}.txt", DateTime.UtcNow.ToString("yyyy-MM-dd")));
            }
        }
    }
}
