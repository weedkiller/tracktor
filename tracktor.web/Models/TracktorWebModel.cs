﻿// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tracktor.service;

namespace tracktor.web.Models
{
    public class TracktorWebModel
    {
        public TSummaryModelDto SummaryModel { get; set; }
        public TEntriesModelDto EntriesModel { get; set; }
        public TStatusModelDto StatusModel { get; set; }
        public TEditModelDto EditModel { get; set; }

        public WebReportModel ReportModel { get; set; }
    }

    public class WebReportDay
    {
        public WebReportDay(DateTime day, double contrib, int reportMonth)
        {
            Day = day.Day;
            Contrib = contrib;
            InFocus = (day.Month == reportMonth);
        }
        public int Day { get; set; }
        public double Contrib { get; set; }
        public bool InFocus { get; set; }
    }

    public class WebReportWeek
    {
        public WebReportWeek(DateTime firstDay)
        {
            Days = new List<WebReportDay>();
            Contrib = 0;
        }
        public List<WebReportDay> Days { get; set; }
        public double Contrib { get; set; }
    }

    public class WebReportTaskContrib
    {
        public string TaskName { get; set; }
        public double Contrib { get; set; }
    }

    public class WebReportProjectContrib
    {
        public string ProjectName { get; set; }
        public List<WebReportTaskContrib> TaskContribs { get; set; }
        public double Contrib { get; set; }
    }

    public class WebReportModel
    {
        public static WebReportModel Create(TSummaryModelDto summaryModel, DateTime reportMonth)
        {
            var projects = new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "(all)") };
            projects.AddRange(summaryModel.Projects.OrderBy(p => p.DisplayOrder).ThenBy(p => p.TProjectID).Select(p => new KeyValuePair<int, string>(p.TProjectID, p.Name)));
            var tasks = new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "(all)") };
            tasks.AddRange(summaryModel.Projects.SelectMany(p => p.TTasks.Select(t => new { p, t})).Where(t => !t.t.IsObsolete).OrderBy(t => t.p.DisplayOrder).ThenBy(t => t.t.DisplayOrder).Select(t => new KeyValuePair<int, string>(t.t.TTaskID, String.Format("{0}.{1}", t.p.Name.Substring(0,3).ToUpper(), t.t.Name))));
            return new WebReportModel()
            {
                Projects = projects,
                Tasks = tasks,
                Years = Enumerable.Range(2013, DateTime.Today.Year - 2012).ToList(),
                Months = Enumerable.Range(1, 12).ToList(),
                Report = new List<WebReportWeek>(),
                ProjectContribs = new List<WebReportProjectContrib>(),
                Contrib = 0,
                SelectedYear = reportMonth.Year,
                SelectedMonth = reportMonth.Month
            };
        }
        public List<KeyValuePair<int, string>> Projects { get; set; }
        public List<KeyValuePair<int, string>> Tasks { get; set; }
        public List<int> Years { get; set; }
        public List<int> Months { get; set; }
        public List<WebReportWeek> Report { get; set; }
        public List<WebReportProjectContrib> ProjectContribs { get; set; }
        public double Contrib { get; set; }

        public int SelectedYear;
        public int SelectedMonth;
    }
}