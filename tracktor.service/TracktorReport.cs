using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //Debug.Assert(day.Kind == DateTimeKind.Local, "Day is not in local time!");
            if(day < StartDate || day > EndDate)
            {
                return; // ignore
            }

            if (!DayContribs.ContainsKey(day))
            {
                DayContribs[day] = amount;
            }
            else
            {
                DayContribs[day] += amount;
            }
            if (!TaskContribs.ContainsKey(taskId))
            {
                TaskContribs[taskId] = amount;
            }
            else
            {
                TaskContribs[taskId] += amount;
            }
        }

        protected double GetTotalContribBetween(DateTime startDate, DateTime endDate)
        {
            return DayContribs.Where(d => d.Key >= startDate && d.Key <= endDate).Sum(c => c.Value);
        }

        public TContribDto GetContrib()
        {
            return new TContribDto() {
                Today = GetTotalContribBetween(EndDate, EndDate),
                ThisWeek = GetTotalContribBetween(EndDate.StartOfWeek(DayOfWeek.Monday), EndDate),
                ThisMonth = GetTotalContribBetween(new DateTime(EndDate.Year, EndDate.Month, 1), EndDate),
            };
        }
    }
}
