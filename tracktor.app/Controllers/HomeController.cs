using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Microsoft.AspNetCore.Identity;
using tracktor.app.Models;

namespace tracktor.app.Controllers
{
    public class HomeController : TracktorControllerBase
    {
        public HomeController(ITracktorService service, UserManager<ApplicationUser> userManager) : base(service, userManager)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }

        [Authorize]
        public FileResult CSV()
        {
            try
            {
                var entries = _service.GetEntriesModelAsync(GetContext(Request.HttpContext), null, null, 0, 0, 99999).Result;

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
