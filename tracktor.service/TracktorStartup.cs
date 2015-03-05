using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tracktor.model.DAL;

namespace tracktor.service
{
    public static class TracktorStartup
    {
        public static void Initialize()
        {
            Mapper.CreateMap<TProject, TProjectDto>();
            Mapper.CreateMap<TTask, TTaskDto>();
            Mapper.CreateMap<TEntry, TEntryDto>().AfterMap((src, dst) => dst.Contrib = (decimal)(((src.EndDate.HasValue ? src.EndDate.Value : DateTime.UtcNow) - src.StartDate).TotalSeconds));

            Mapper.CreateMap<TracktorReport, TracktorReportDto>();
        }
    }
}