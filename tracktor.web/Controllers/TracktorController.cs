﻿using System;
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

namespace tracktor.web.Controllers
{
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
                return new TContextDto {
                    TUserID = userID,
                    UTCOffset = (int)userTimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes
                };
            }
        }

        public TModelDto GetModel()
        {
            return _service.GetModel(Context);
        }

        public TTaskDto UpdateTask(TTaskDto task)
        {
            return _service.UpdateTask(Context, task);
        }

        public TProjectDto UpdateProject(TProjectDto project)
        {
            return _service.UpdateProject(Context, project);
        }

        public TEntryDto UpdateEntry(TEntryDto entry)
        {
            return _service.UpdateEntry(Context, entry);
        }

        public List<TEntryDto> GetEntries(DateTime? startDate, DateTime? endDate, int projectID, int maxEntries)
        {
            return _service.GetEntries(Context, startDate, endDate, projectID, maxEntries);
        }

        public TracktorReportDto GetReport(DateTime? startDate, DateTime? endDate, int projectID)
        {
            return _service.GetReport(Context, startDate, endDate, projectID);
        }

        public TModelDto StopTask(int currentTaskID)
        {
            _service.StopTask(Context, currentTaskID);
            return _service.GetModel(Context);
        }

        public TModelDto StartTask(int newTaskID)
        {
            _service.StartTask(Context, newTaskID);
            return _service.GetModel(Context);
        }

        public TModelDto SwitchTask(int currentTaskID, int newTaskId)
        {
            _service.SwitchTask(Context, currentTaskID, newTaskId);
            return _service.GetModel(Context);
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
