using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using tracktor.model;
using tracktor.model.DAL;

namespace tracktor.service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class TracktorService : ITracktorService, IDisposable
    {
        private TracktorContext _db = new TracktorContext();

        #region ITracktorService

        public int CreateUser(string userName)
        {
            try
            {
                var newUser = new TUser { Name = userName, LastTaskID = 0, CurrentState = TState.Idle };
                _db.TUsers.Add(newUser);
                _db.SaveChanges();
                return newUser.TUserID;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TModelDto GetModel(TContextDto context)
        {
            try
            {
                TModelDto model = null;
                model = new TModelDto {
                    Projects = _db.TProjects.Where(p => p.TUserID == context.TUserID).ToList().Select(p => Mapper.Map<TProjectDto>(p)).ToList()
                };
                using (var calc = new TracktorCalculator(context))
                {
                    calc.CalculateContribs(null, calc.DateOrLocalNow(null), model);
                }
                return model;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TTaskDto UpdateTask(TContextDto context, TTaskDto task)
        {
            try
            {
                return UpdateObject<TTaskDto, TTask>(context, task, _db.TTasks, (d => d.TTaskID), (t => t.TTaskID));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TProjectDto UpdateProject(TContextDto context, TProjectDto project)
        {
            try
            {
                return UpdateObject<TProjectDto, TProject>(context, project, _db.TProjects, (d => d.TProjectID), (t => t.TProjectID));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TEntryDto UpdateEntry(TContextDto context, TEntryDto entry)
        {
            try
            {
                return UpdateObject<TEntryDto, TEntry>(context, entry, _db.TEntries, (d => d.TEntryID), (t => t.TEntryID));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public List<TEntryDto> GetEntries(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID, int maxEntries)
        {
            try
            {
                using (var calculator = new TracktorCalculator(context))
                {
                    return calculator.GetEntries(startDate, calculator.DateOrLocalNow(endDate), projectID, maxEntries).Select(e => Mapper.Map<TEntryDto>(e)).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TracktorReportDto GetReport(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID)
        {
            try
            {
                using (var calculator = new TracktorCalculator(context))
                {
                    return Mapper.Map<TracktorReportDto>(calculator.GetReport(startDate, calculator.DateOrLocalNow(endDate), projectID));
                }
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TModelDto StopTask(TContextDto context, int currentTaskID)
        {
            try
            {
                using (var states = new TracktorStates(context))
                {
                    states.Stop(currentTaskID);
                }
                return GetModel(context);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TModelDto StartTask(TContextDto context, int newTaskID)
        {
            try
            {
                using (var states = new TracktorStates(context))
                {
                    states.Start(newTaskID);
                }
                return GetModel(context);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TModelDto SwitchTask(TContextDto context, int currentTaskID, int newTaskID)
        {
            try
            {
                using (var states = new TracktorStates(context))
                {
                    states.Stop(currentTaskID);
                    states.Start(newTaskID);
                }
                return GetModel(context);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region Helpers

        protected D UpdateObject<D, T>(TContextDto context, D dto, DbSet<T> dbSet, Func<D, int> dtoID, Func<T, int> tID)
            where D : new()
            where T : class, new()
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

        protected bool IsAllowed(TContextDto context, object storedObject)
        {
            if (storedObject is TEntry)
            {
                return IsAllowed(context, (storedObject as TEntry).TTask);
            }
            else if (storedObject is TTask)
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

        #endregion

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
