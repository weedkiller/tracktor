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

        private TContextDto Context
        {
            get
            {
                var userID = Int32.Parse(this.Request.GetOwinContext().Authentication.User.FindFirst("TUserID").Value);
                var user = Request.GetOwinContext().GetUserManager<ApplicationUserManager>().Users.Where(u => u.TUserID == userID).FirstOrDefault();
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
        }

        [HttpGet]
        public TracktorWebModel GetModel(bool updateOnly = false)
        {
            return new TracktorWebModel
            {
                SummaryModel = _service.GetSummaryModel(Context),
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
                ReportModel = updateOnly ? null : new TReportModelDto
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    DayContribs = new Dictionary<DateTime, double> { { DateTime.UtcNow.Date, 0 } },
                    TaskContribs = new Dictionary<int, double> { { 0, 0 } }
                }
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
        public TracktorWebModel GetReportModel(DateTime? startDate, DateTime? endDate, int projectID)
        {
            return new TracktorWebModel
            {
                ReportModel = _service.GetReportModel(Context, startDate, endDate, projectID)
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
