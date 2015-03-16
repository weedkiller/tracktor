using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;
using tracktor.service;
using tracktor.web.Models;

namespace tracktor.web.Controllers
{
    public class TracktorEntryAction
    {
        public int currentTaskID { get; set; }
        public int newTaskID { get; set; }
    }

    [Authorize]
    public class TracktorController : ApiController
    {
        private ITracktorService _service = new TracktorService(); // load locally, but can replace with remote invocation

        public static TContextDto GetContext(IOwinContext owinContext)
        {
            var userID = Int32.Parse(owinContext.Authentication.User.FindFirst("TUserID").Value);
            var user = owinContext.GetUserManager<ApplicationUserManager>().Users.Where(u => u.TUserID == userID).FirstOrDefault();
            var userTimeZone = TimeZoneInfo.Utc;
            if (!string.IsNullOrWhiteSpace(user.TimeZone))
            {
                userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
            }
            return new TContextDto
            {
                TUserID = userID,
                UTCOffset = -(int)userTimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes
            };
        }

        private TContextDto Context
        {
            get
            {
                return GetContext(Request.GetOwinContext());
            }
        }

        [HttpGet]
        public TracktorWebModel GetModel(bool updateOnly = false)
        {
            var summaryModel = _service.GetSummaryModel(Context);
            return new TracktorWebModel
            {
                SummaryModel = summaryModel,
                EntriesModel = _service.GetEntriesModel(Context, null, null, 0, 0, 20),
                StatusModel = _service.GetStatusModel(Context),
                EditModel = updateOnly ? null : new TEditModelDto
                {
                    Entry = new TEntryDto
                    {
                        EndDate = DateTime.UtcNow,
                        StartDate = DateTime.UtcNow,
                        IsDeleted = false,
                        InProgress = false,
                        TaskName = "",
                        ProjectName = "",
                        TTaskID = 0,
                        TEntryID = 0,
                        Contrib = 0,
                    }
                },
                ReportModel = updateOnly ? null : WebReportModel.Create(summaryModel, DateTime.UtcNow)
            };
        }

        [HttpPost]
        public TTaskDto UpdateTask(TTaskDto task)
        {
            return _service.UpdateTask(Context, task);
        }

        [HttpPost]
        public TProjectDto UpdateProject(TProjectDto project)
        {
            return _service.UpdateProject(Context, project);
        }

        [HttpPost]
        public TracktorWebModel UpdateEntry(TEntryDto entry)
        {
            return new TracktorWebModel
            {
                EditModel = new TEditModelDto
                {
                    Entry = _service.UpdateEntry(Context, entry)
                }
            };
        }

        [HttpGet]
        public TracktorWebModel GetEntry(int entryID)
        {
            return new TracktorWebModel
            {
                EditModel = new TEditModelDto
                {
                    Entry = _service.GetEntry(Context, entryID)
                }
            };
        }

        [HttpGet]
        public TracktorWebModel GetEntriesModel(DateTime? startDate, DateTime? endDate, int projectID, int startNo, int maxEntries)
        {
            return new TracktorWebModel
            {
                EntriesModel = _service.GetEntriesModel(Context, startDate, endDate, projectID, startNo, maxEntries)
            };
        }

        [HttpGet]
        public TracktorWebModel GetWebReport(int year, int month, int projectID)
        {
            var summaryModel = _service.GetSummaryModel(Context);
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Local);
            var endDate = startDate.AddMonths(1);
            var reportModel = _service.GetReportModel(Context, startDate, endDate, projectID);
            var webReport = WebReportModel.Create(summaryModel, startDate);

            var reportStart = startDate.StartOfWeek(DayOfWeek.Monday);
            var reportEnd = endDate.StartOfWeek(DayOfWeek.Monday).AddDays(6);
            var rollingDate = reportStart;
            WebReportWeek currentWeek = null;
            while (rollingDate <= reportEnd)
            {
                if (rollingDate.DayOfWeek == DayOfWeek.Monday)
                {
                    if (currentWeek != null)
                    {
                        webReport.Report.Add(currentWeek);
                    }
                    currentWeek = new WebReportWeek(rollingDate);
                }
                double contrib = 0;
                if (reportModel.DayContribs.ContainsKey(rollingDate))
                {
                    contrib = reportModel.DayContribs[rollingDate];
                }
                var currentDay = new WebReportDay(rollingDate, contrib, month);
                currentWeek.Days.Add(currentDay);
                if (currentDay.InFocus)
                {
                    currentWeek.Contrib += contrib;
                    webReport.Contrib += contrib;
                }

                rollingDate = rollingDate.AddDays(1);
            }
            if (currentWeek != null)
            {
                webReport.Report.Add(currentWeek);
            }

            foreach (var project in summaryModel.Projects.Where(p => projectID == 0 || p.TProjectID == projectID))
            {
                var projectContrib = new WebReportProjectContrib
                {
                    ProjectName = project.Name,
                    TaskContribs = new List<WebReportTaskContrib>(),
                    Contrib = 0
                };
                foreach (var task in project.TTasks)
                {
                    var taskContrib = new WebReportTaskContrib
                    {
                        TaskName = task.Name,
                        Contrib = 0
                    };
                    if (reportModel.TaskContribs.ContainsKey(task.TTaskID))
                    {
                        taskContrib.Contrib = reportModel.TaskContribs[task.TTaskID];
                    }
                    projectContrib.TaskContribs.Add(taskContrib);
                    projectContrib.Contrib += taskContrib.Contrib;
                }
                webReport.ProjectContribs.Add(projectContrib);
            }

            return new TracktorWebModel
            {
                ReportModel = webReport
            };
        }

        [HttpPost]
        public TracktorWebModel StopTask(TracktorEntryAction actionModel)
        {
            _service.StopTask(Context, actionModel.currentTaskID);
            return GetModel(true);
        }

        [HttpPost]
        public TracktorWebModel StartTask(TracktorEntryAction actionModel)
        {
            _service.StartTask(Context, actionModel.newTaskID);
            return GetModel(true);
        }

        [HttpPost]
        public TracktorWebModel SwitchTask(TracktorEntryAction actionModel)
        {
            _service.SwitchTask(Context, actionModel.currentTaskID, actionModel.newTaskID);
            return GetModel(true);
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
