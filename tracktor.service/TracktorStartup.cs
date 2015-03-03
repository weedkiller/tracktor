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
            Mapper.CreateMap<TEntry, TEntryDto>();

            Mapper.CreateMap<TracktorReport, TracktorReportDto>();
        }
    }
}