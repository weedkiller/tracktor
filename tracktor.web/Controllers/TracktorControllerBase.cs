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
    public class TracktorControllerBase : ApiController
    {
        protected ITracktorService _service;

        public TracktorControllerBase(ITracktorService service)
        {
            _service = service;
        }

        public static TimeZoneInfo GetUserTimezone(IOwinContext owinContext, out int userID)
        {
            int _userID = Int32.Parse(owinContext.Authentication.User.FindFirst("TUserID").Value);
            var user = owinContext.GetUserManager<ApplicationUserManager>().Users.Where(u => u.TUserID == _userID).FirstOrDefault();
            var userTimeZone = TimeZoneInfo.Utc;
            if (!string.IsNullOrWhiteSpace(user.TimeZone))
            {
                userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
            }
            userID = _userID;
            return userTimeZone;
        }

        public static TContextDto GetContext(IOwinContext owinContext)
        {
            int userID;
            var userTimeZone = GetUserTimezone(owinContext, out userID);
            return new TContextDto
            {
                TUserID = userID,
                UTCOffset = -(int)userTimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes
            };
        }

        protected TContextDto Context
        {
            get
            {
                return GetContext(Request.GetOwinContext());
            }
        }

        protected TracktorWebModel GenerateWebModel(bool updateOnly = false)
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
    }
}
