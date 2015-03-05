using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tracktor.model.DAL;

namespace tracktor.service
{
    public class TracktorStartup
    {
        public static void AppInitialize()
        {
            Mapper.CreateMap<TProject, TProjectDto>();
            Mapper.CreateMap<TTask, TTaskDto>().AfterMap((src, dst) => dst.Contrib = new TContribDto { Today = 0, ThisWeek = 0, ThisMonth = 0 });
            Mapper.CreateMap<TEntry, TEntryDto>().AfterMap((src, dst) => dst.Contrib = ((src.EndDate.HasValue ? src.EndDate.Value : DateTime.UtcNow) - src.StartDate).TotalSeconds);

            Mapper.CreateMap<TracktorReport, TracktorReportDto>();
        }
    }
}