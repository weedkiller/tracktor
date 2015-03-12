using AutoMapper;
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

        public DateTime? ToUtc(DateTime? inputDate)
        {
            if (inputDate.HasValue)
            {
                Debug.Assert(inputDate.Value.Kind != DateTimeKind.Utc, "Input Date is already UTC!");
                return new DateTime(inputDate.Value.Ticks, DateTimeKind.Utc).AddMinutes(mContext.UTCOffset);
            }
            return null;
        }

        public DateTime? ToLocal(DateTime? inputDate)
        {
            if (inputDate.HasValue)
            {
                Debug.Assert(inputDate.Value.Kind != DateTimeKind.Local, "Input Date is already local!");
                return new DateTime(inputDate.Value.Ticks, DateTimeKind.Local).AddMinutes(-mContext.UTCOffset);
            }
            return null;
        }

        public List<TEntry> GetEntries(DateTime? startDate, DateTime endDate, int projectID, int startNo = 0, int maxEntries = -1)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be UTC!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be UTC!");

            if (startDate.HasValue && startDate > endDate)
            {
                throw new Exception("Invalid start date after end date!");
            }
            DateTime? utcStart = ToUtc(startDate);
            DateTime? utcEnd = ToUtc(endDate);
            return _db.TEntries.AsNoTracking().Where(e => (e.TTask.TProjectID == projectID || projectID == 0) && e.TTask.TProject.TUserID == mContext.TUserID)
                .Where(e => !(utcStart.HasValue && e.EndDate.HasValue && e.EndDate < utcStart)
                            && !(e.StartDate > utcEnd))
                .OrderByDescending(e => e.EndDate.HasValue ? e.EndDate.Value : e.StartDate)
                .Skip(startNo)
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

        public TEntryDto EnrichTEntry(TEntryDto entryDto, TEntry entry)
        {
            var dto = entryDto;
            if (dto == null)
            {
                dto = Mapper.Map<TEntryDto>(entry);
            }
            else
            {
                Mapper.Map<TEntry, TEntryDto>(entry, entryDto);
            }
            dto.StartDate = ToLocal(entry.StartDate).Value;
            dto.EndDate = ToLocal(entry.EndDate);
            if (string.IsNullOrWhiteSpace(dto.TaskName))
            {
                dto.TaskName = entry.TTask.Name;
            }
            if (string.IsNullOrWhiteSpace(dto.ProjectName))
            {
                dto.ProjectName = entry.TTask.TProject.Name;
            }
            if (dto.Contrib == 0)
            {
                dto.Contrib = (DateOrLocalNow(dto.EndDate) - dto.StartDate).TotalSeconds;
            }
            return dto;
        }

        public TStatusModelDto BuildStatusModel()
        {
            var entries = _db.TEntries.Where(e => e.TTask.TProject.TUserID == mContext.TUserID);
            var entryInProgress = entries.SingleOrDefault(e => !e.EndDate.HasValue);
            var latestEntry = entries.OrderByDescending(e => e.StartDate).FirstOrDefault();

            return new TStatusModelDto
            {
                InProgress = (entryInProgress != null),
                LatestEntry = EnrichTEntry(null, latestEntry),
                TTaskInProgress = (entryInProgress != null) ? Mapper.Map<TTaskDto>(entryInProgress.TTask) : null
            };
        }

        public TSummaryModelDto BuildSummaryModel(DateTime? startDate, DateTime endDate)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be UTC!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be UTC!");

            var model = new TSummaryModelDto
            {
                Projects = _db.TProjects.Where(p => p.TUserID == mContext.TUserID).ToList().Select(p => Mapper.Map<TProjectDto>(p)).ToList()
            };

            var taskToProject = model.Projects.SelectMany(p => p.TTasks.Select(t => new KeyValuePair<int, int>(t.TTaskID, t.TProjectID))).ToDictionary(t => t.Key, p => p.Value);
            List<TEntry> entries = GetEntries(startDate, endDate, 0);
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
                                if (!entry.EndDate.HasValue)
                                {
                                    taskDto.InProgress = true;
                                    projectDto.InProgress = true;
                                    model.InProgress = true;
                                }
                            }
                            taskDto.Contrib = report.GetContrib();
                        }
                    }
                }
            }

            return model;
        }

        public List<TEntryDto> CalculateEntryContribs(List<TEntry> entries, DateTime? startDate, DateTime endDate)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be UTC!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be UTC!");

            var dtos = new List<TEntryDto>();
            var descriptions = _db.TTasks.Where(t => t.TProject.TUserID == mContext.TUserID).Select(t => new { TTaskID = t.TTaskID, TaskName = t.Name, ProjectName = t.TProject.Name }).
                ToDictionary(t => t.TTaskID, t => t);
            var report = new TracktorReport(startDate, endDate);

            foreach (var entry in entries)
            {
                var entryDto = Mapper.Map<TEntryDto>(entry);
                var description = descriptions[entry.TTaskID];
                entryDto.TaskName = description.TaskName;
                entryDto.ProjectName = description.ProjectName;
                entryDto.Contrib = BucketEntry(entry, report);
                entryDto.InProgress = (!entry.EndDate.HasValue);
                dtos.Add(entryDto);
            }

            return dtos;
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
