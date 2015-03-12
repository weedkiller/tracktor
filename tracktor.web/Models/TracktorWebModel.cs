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
        public TReportModelDto ReportModel { get; set; }
        public TStatusModelDto StatusModel { get; set; }
        public TEditModelDto EditModel { get; set; }
    }
}