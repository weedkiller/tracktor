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
        private ITracktorService _service = new TracktorService(); // load locally, but can replace with remote invocation

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

        [Authorize]
        public FileResult CSV()
        {
            var context = TracktorController.GetContext(Request.GetOwinContext());
            var entries = _service.GetEntriesModel(context, null, null, 0, 0, 99999);

            var csvFile = new StringBuilder();
            csvFile.AppendLine("Start,End,Project,Task,Contribution,InProgress");
            foreach(var entry in entries.Entries)
            {
                csvFile.AppendLine(
                    String.Format(
                        "{0},{1},{2},{3},{4},{5}",
                        entry.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        entry.EndDate.HasValue ? entry.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        entry.ProjectName,
                        entry.TaskName,
                        (entry.Contrib / 60).ToString(),
                        entry.InProgress ? "Yes" : "No"));
            }
            return File(
                System.Text.Encoding.UTF8.GetBytes(csvFile.ToString()),
                "text/csv",
                string.Format("tracktor tasks {0}.csv", DateTime.UtcNow.ToString("yyyy-MM-dd")));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_service != null && _service is IDisposable)
                {
                    (_service as IDisposable).Dispose();
                    _service = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
