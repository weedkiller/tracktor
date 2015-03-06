using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tracktor.model;
using tracktor.model.DAL;

namespace tracktor.service
{
    public class TracktorCalculator : IDisposable
    {
        private readonly static int MaxEntries = 99999;

        private TracktorContext _db = new TracktorContext();
        protected TContextDto mContext;

        public TracktorCalculator(TContextDto context)
        {
            mContext = context;
        }

        public DateTime DateOrLocalNow(DateTime? inputDate)
        {
            if (!inputDate.HasValue)
            {
                return ToLocal(DateTime.UtcNow).Value;
            }
            return inputDate.Value;
        }

        protected DateTime? ToUtc(DateTime? inputDate)
        {
            if (inputDate.HasValue)
            {
                Debug.Assert(inputDate.Value.Kind != DateTimeKind.Utc, "Input Date is already UTC!");
                return new DateTime(inputDate.Value.Ticks, DateTimeKind.Utc).AddMinutes(mContext.UTCOffset);
            }
            return null;
        }

        protected DateTime? ToLocal(DateTime? inputDate)
        {
            if (inputDate.HasValue)
            {
                Debug.Assert(inputDate.Value.Kind != DateTimeKind.Local, "Input Date is already local!");
                return new DateTime(inputDate.Value.Ticks, DateTimeKind.Local).AddMinutes(-mContext.UTCOffset);
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
            DateTime? utcStart = ToUtc(startDate);
            DateTime? utcEnd = ToUtc(endDate);
            return _db.TEntries.Where(e => (e.TTask.TProjectID == projectID || projectID == 0) && e.TTask.TProject.TUserID == mContext.TUserID)
                .Where(e => !(utcStart.HasValue && e.EndDate.HasValue && e.EndDate < utcStart)
                            && !(e.StartDate > utcEnd))
                .OrderByDescending(e => e.EndDate.HasValue ? e.EndDate.Value : e.StartDate)
                .Take(maxEntries <= 0 ? MaxEntries : maxEntries).ToList();
        }

        protected double BucketEntry(TEntry entry, TracktorReport report)
        {
            DateTime localStart = ToLocal(entry.StartDate).Value;
            DateTime localEnd = ToLocal(entry.EndDate.HasValue ? entry.EndDate : DateTime.UtcNow).Value;
            DateTime firstDay = localStart.Date;
            DateTime lastDay = localEnd.Date;
            DateTime currentDay = firstDay;
            double totalContrib = 0;
            while (currentDay <= lastDay)
            {
                DateTime nextDay = currentDay.AddDays(1);
                DateTime periodStart = currentDay > localStart ? currentDay : localStart;
                DateTime periodEnd = nextDay > localEnd ? localEnd : nextDay;
                var periodContrib = (periodEnd - periodStart).TotalSeconds;
                report.AddContrib(currentDay, entry.TTaskID, periodContrib);
                currentDay = nextDay;
                totalContrib += periodContrib;
            }
            return totalContrib;
        }

        public TracktorReport GetReport(DateTime? startDate, DateTime endDate, int projectID)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be UTC!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be UTC!");

            List<TEntry> entries = GetEntries(startDate, endDate, projectID);
            var report = new TracktorReport(startDate, endDate);
            foreach (var entry in entries)
            {
                BucketEntry(entry, report);
            }
            return report;
        }

        public void CalculateContribs(DateTime? startDate, DateTime endDate, TModelDto model)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be UTC!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be UTC!");
            var taskToProject = model.Projects.SelectMany(p => p.TTasks.Select(t => new KeyValuePair<int, int>(t.TTaskID, t.TProjectID))).ToDictionary(t => t.Key, p => p.Value);
            List<TEntry> entries = GetEntries(startDate, endDate, 0);
            var entriesById = model.Entries.ToDictionary(e => e.TEntryID, e => e);
            foreach (var taskEntry in entries.GroupBy(e => e.TTaskID))
            {
                if (taskToProject.ContainsKey(taskEntry.Key))
                {
                    var projectDto = model.Projects.SingleOrDefault(p => p.TProjectID == taskToProject[taskEntry.Key]);
                    if (projectDto != null)
                    {
                        var taskDto = projectDto.TTasks.SingleOrDefault(t => t.TTaskID == taskEntry.Key);
                        if (taskDto != null)
                        {
                            var report = new TracktorReport(startDate, endDate);
                            foreach (var entry in taskEntry)
                            {
                                var totalContrib = BucketEntry(entry, report);

                                // find it individually
                                TEntryDto entryDto;
                                if (entriesById.TryGetValue(entry.TEntryID, out entryDto))
                                {
                                    entryDto.Contrib = totalContrib;
                                    if(!entry.EndDate.HasValue)
                                    {
                                        entryDto.InProgress = true;
                                        taskDto.InProgress = true;
                                        projectDto.InProgress = true;
                                    }
                                }
                            }
                            taskDto.Contrib = report.GetContrib();
                        }
                    }
                }
            }
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
