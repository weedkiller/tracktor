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
            Mapper.CreateMap<TProject, TProjectDto>().AfterMap((src, dst) =>
                dst.InProgress = false);

            Mapper.CreateMap<TTask, TTaskDto>().AfterMap((src, dst) => {
                dst.Contrib = new TContribDto { Today = 0, ThisWeek = 0, ThisMonth = 0 };
                dst.InProgress = false;
            });

            Mapper.CreateMap<TEntry, TEntryDto>().AfterMap((src, dst) => {
                dst.Contrib = 0;
                dst.InProgress = false;
                dst.IsDeleted = false;
            });

            Mapper.CreateMap<TracktorReport, TReportModelDto>();
        }
    }
}