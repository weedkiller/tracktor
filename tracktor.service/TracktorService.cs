using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using tracktor.model.DAL;

namespace tracktor.service
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerCall)]
    public class TracktorService : ITracktorService, IDisposable
    {
        private TracktorContext _db = new TracktorContext();

        public CModel GetModel(CContext context)
        {
            using (var db = new TracktorContext())
            {
                return new CModel
                {
                    Projects = db.TProjects.Where(p => p.TUserID == context.TUserID).ToList().Select(p => Mapper.Map<TProjectDto>(p)).ToList()
                };
            }
        }

        public D UpdateObject<D, T>(CContext context, D dto, DbSet<T> dbSet, Func<D, int> dtoID, Func<T, int> tID) where D : new() where T : class, new()
        {
            var storedObject = dbSet.SingleOrDefault(t => tID(t) == dtoID(dto));
            if (storedObject == null)
            {
                if (dtoID(dto) != 0)
                {
                    throw new Exception(String.Format("Unable to find existing {0} object with ID {1}!", typeof(T).Name, dtoID(dto)));
                }
                storedObject = new T();
                dbSet.Add(storedObject);
            }
            else if (!IsAllowed(context, storedObject))
            {
                throw new Exception(String.Format("User ID {0} is not allowed to modify {1} object with ID {2}!", context.TUserID, typeof(T).Name, dtoID(dto)));
            }
            Mapper.Map<D, T>(dto, storedObject);
            _db.SaveChanges();
            return Mapper.Map<D>(storedObject);
        }

        protected bool IsAllowed(CContext context, object storedObject)
        {
            if(storedObject is TEntry)
            {
                return IsAllowed(context, (storedObject as TEntry).TTask);
            }
            else if(storedObject is TTask)
            {
                return IsAllowed(context, (storedObject as TTask).TProject);
            }
            else if (storedObject is TProject)
            {
                return context.TUserID == (storedObject as TProject).TUserID;
            }
            else
            {
                return true;
            }
        }

        public TTaskDto UpdateTask(CContext context, TTaskDto task)
        {
            return UpdateObject<TTaskDto, TTask>(context, task, _db.TTasks, (d => d.TTaskID), (t => t.TTaskID));
        }

        public TProjectDto UpdateProject(CContext context, TProjectDto project)
        {
            return UpdateObject<TProjectDto, TProject>(context, project, _db.TProjects, (d => d.TProjectID), (t => t.TProjectID));
        }

        public TEntryDto UpdateEntry(CContext context, TEntryDto entry)
        {
            return UpdateObject<TEntryDto, TEntry>(context, entry, _db.TEntries, (d => d.TEntryID), (t => t.TEntryID));
        }

        public List<TEntryDto> GetEntries(CContext context, DateTime? startDate, DateTime endDate, int projectID, int maxEntries)
        {
            using(var calculator = new TracktorCalculator(context))
            {
                return calculator.GetEntries(startDate, endDate, projectID, maxEntries).Select(e => Mapper.Map<TEntryDto>(e)).ToList();
            }
        }

        public TracktorReportDto GetReport(CContext context, DateTime? startDate, DateTime endDate, int projectID)
        {
            using (var calculator = new TracktorCalculator(context))
            {
                return Mapper.Map<TracktorReportDto>(calculator.GetReport(startDate, endDate, projectID));
            }
        }

        public TEntryDto StopTask(CContext context, int taskID)
        {
            throw new NotImplementedException();
        }

        public bool StartTask(CContext context, int taskID)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }
        }

        #endregion
    }
}
