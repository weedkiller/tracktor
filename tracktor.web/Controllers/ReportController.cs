// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

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
    [Authorize]
    public class ReportController : TracktorControllerBase
    {
        public ReportController(ITracktorService service) : base(service)
        {
        }

        [Route("report")]
        [HttpGet]
        public TracktorWebModel Get(int year, int month, int projectID, int taskID)
        {
            var summaryModel = _service.GetSummaryModel(Context);
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Local);
            var endDate = startDate.AddMonths(1);
            var reportModel = _service.GetReportModel(Context, startDate, endDate, projectID, taskID);
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
                foreach (var task in project.TTasks.Where(t => taskID == 0 || t.TTaskID == taskID))
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
    }
}
