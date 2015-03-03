using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tracktor.model.DAL;

namespace tracktor.service
{
    public class TracktorReport
    {
        public Dictionary<DateTime, double> DayContribs { get; set; }
        public Dictionary<int, double> TaskContribs { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TracktorReport(DateTime? startDate, DateTime endDate)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be Utc!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be Utc!");

            StartDate = startDate.HasValue ? (DateTime?)startDate.Value.Date : null;
            EndDate = endDate.Date;
            DayContribs = new Dictionary<DateTime, double>();
            TaskContribs = new Dictionary<int, double>();
        }

        public void AddContrib(DateTime day, int taskId, double amount)
        {
            Debug.Assert(day.Kind == DateTimeKind.Local, "Day is not in local time!");

            if(!DayContribs.ContainsKey(day))
            {
                DayContribs[day] = amount;
            }
            else
            {
                DayContribs[day] += amount;
            }
            if(!TaskContribs.ContainsKey(taskId))
            {
                TaskContribs[taskId] = amount;
            }
            else
            {
                TaskContribs[taskId] += amount;
            }
        }
    }

    public class TracktorCalculator : IDisposable
    {
        private readonly static int MaxEntries = 99999;

        private TracktorContext _db = new TracktorContext();
        protected CContext mContext;

        public TracktorCalculator(CContext context)
        {
            mContext = context;
        }

        protected DateTime? ToUtc(DateTime? inputDate, int offset)
        {            
            if(inputDate.HasValue)
            {
                Debug.Assert(inputDate.Value.Kind != DateTimeKind.Utc, "Input Date is already UTC!");
                return new DateTime(inputDate.Value.Ticks, DateTimeKind.Utc).AddMinutes(offset);
            }
            return null;
        }

        protected DateTime? ToLocal(DateTime? inputDate, int offset)
        {
            if (inputDate.HasValue)
            {
                Debug.Assert(inputDate.Value.Kind != DateTimeKind.Local, "Input Date is already local!");
                return new DateTime(inputDate.Value.Ticks, DateTimeKind.Local).AddMinutes(-offset);
            }
            return null;
        }

        public List<TEntry> GetEntries(DateTime? startDate, DateTime endDate, int projectID, int maxEntries = 0)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be UTC!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be UTC!");

            if (startDate.HasValue && startDate > endDate)
            {
                throw new Exception("Invalid start date after end date!");
            }
            DateTime? utcStart = ToUtc(startDate, mContext.UTCOffset);
            DateTime? utcEnd = ToUtc(endDate, mContext.UTCOffset);
            return _db.TEntries.Where(e => (e.TTask.TProjectID == projectID || projectID == 0) && e.TTask.TProject.TUserID == mContext.TUserID)
                .Where(e => !(utcStart.HasValue && e.EndDate < utcStart)
                            && !(e.StartDate > utcEnd))
                .OrderByDescending(e => e.EndDate.HasValue ? e.EndDate.Value : e.StartDate)
                .Take(maxEntries <= 0 ? MaxEntries : maxEntries).ToList();
        }

        protected void BucketEntry(TEntry entry, TracktorReport report)
        {
            DateTime localStart = ToLocal(entry.StartDate, mContext.UTCOffset).Value;
            DateTime localEnd = ToLocal(entry.EndDate.HasValue ? entry.EndDate : DateTime.UtcNow, mContext.UTCOffset).Value;
            DateTime firstDay = localStart.Date;
            DateTime lastDay = localEnd.Date;
            DateTime currentDay = firstDay;
            while (currentDay <= lastDay)
            {
                DateTime nextDay = currentDay.AddDays(1);
                DateTime periodStart = currentDay > localStart ? currentDay : localStart;
                DateTime periodEnd = nextDay > localEnd ? localEnd : nextDay;
                report.AddContrib(currentDay, entry.TTaskID, (periodEnd - periodStart).TotalMinutes);
                currentDay = nextDay;
            }
        }

        public TracktorReport GetReport(DateTime? startDate, DateTime endDate, int projectID)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be UTC!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be UTC!");

            List<TEntry> entries = GetEntries(startDate, endDate, projectID);
            var report = new TracktorReport(startDate, endDate);
            foreach(var entry in entries)
            {
                BucketEntry(entry, report);
            }
            return report;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }
        }

        #endregion
    }
}
