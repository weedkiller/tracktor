// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
//

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using tracktor.app.Models;

namespace tracktor.app.Controllers
{
    public class TracktorControllerBase : Controller
    {
        protected ITracktorService _service;
        protected UserManager<ApplicationUser> _userManager;

        public TracktorControllerBase(ITracktorService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        public TimeZoneInfo GetUserTimezone(HttpContext httpContext, out int userID)
        {
            var user = _userManager.GetUserAsync(httpContext.User).Result;
            userID = user.TUserID;
            var userTimeZone = TimeZoneInfo.Utc;
            if (!string.IsNullOrWhiteSpace(user.TimeZone))
            {
                userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZone);
            }
            return userTimeZone;
        }

        public TContextDto GetContext(HttpContext httpContext)
        {
            int userID;
            var userTimeZone = GetUserTimezone(httpContext, out userID);
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
                return GetContext(Request.HttpContext);
            }
        }

        protected async Task<TracktorWebModel> GenerateWebModel(bool updateOnly = false)
        {
            var summaryModel = await _service.GetSummaryModelAsync(Context);
            return new TracktorWebModel
            {
                SummaryModel = summaryModel,
                EntriesModel = await _service.GetEntriesModelAsync(Context, null, null, 0, 0, 20),
                StatusModel = await _service.GetStatusModelAsync(Context),
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
