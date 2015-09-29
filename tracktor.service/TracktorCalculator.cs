// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

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
    public class TracktorCalculator
    {
        private readonly static int s_maxEntries = 99999;
        private ITracktorContext _db;

        protected TContextDto mContext;

        public TracktorCalculator(TContextDto context, ITracktorContext db)
        {
            _db = db;
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
                return new DateTime(inputDate.Value.Ticks, DateTimeKind.Unspecified).AddMinutes(-mContext.UTCOffset);
            }
            return null;
        }

        public List<TEntry> GetEntries(DateTime? startDate, DateTime endDate, int projectID, int taskID, int startNo = 0, int maxEntries = -1)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be UTC!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be UTC!");

            if (startDate.HasValue && startDate > endDate)
            {
                throw new Exception("Invalid start date after end date!");
            }
            DateTime? utcStart = ToUtc(startDate);
            DateTime? utcEnd = ToUtc(endDate);
            return _db.TEntries.AsNoTracking().Where(e => (e.TTask.TProjectID == projectID || projectID == 0) && e.TTask.TProject.TUserID == mContext.TUserID &&
                (taskID ==0 || e.TTaskID == taskID))
                .Where(e => !(utcStart.HasValue && e.EndDate.HasValue && e.EndDate < utcStart)
                            && !(e.StartDate > utcEnd))
                .OrderByDescending(e => e.EndDate.HasValue ? e.EndDate.Value : e.StartDate)
                .Skip(startNo)
                .Take(maxEntries <= 0 ? s_maxEntries : maxEntries).ToList();
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

        public TracktorReport GetReport(DateTime? startDate, DateTime endDate, int projectID, int taskID)
        {
            Debug.Assert(!startDate.HasValue || startDate.Value.Kind != DateTimeKind.Utc, "Start Date should not be UTC!");
            Debug.Assert(endDate.Kind != DateTimeKind.Utc, "End Date should not be UTC!");

            List<TEntry> entries = GetEntries(startDate, endDate, projectID, taskID);
            var report = new TracktorReport(startDate, endDate.AddDays(-1));
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
                if (entry == null)
                {
                    return new TEntryDto
                    {
                        Contrib = 0,
                        EndDate = DateTime.UtcNow,
                        StartDate = DateTime.UtcNow,
                        InProgress = false,
                        IsDeleted = true,
                        ProjectName = "",
                        TaskName = "",
                        TEntryID = 0,
                        TTaskID = 0
                    };
                }
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
            List<TEntry> entries = GetEntries(startDate, endDate, 0, 0);
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
                EnrichTEntry(entryDto, entry);
                dtos.Add(entryDto);
            }

            return dtos;
        }
    }
}
