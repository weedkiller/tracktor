using AutoMapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
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
                // use database seeded projects for guest
                var guestEmail = ConfigurationManager.AppSettings["GuestEmail"];
                if (!string.IsNullOrWhiteSpace(guestEmail) && userName.Equals(guestEmail, StringComparison.InvariantCultureIgnoreCase))
                {
                    var firstUser = _db.TUsers.OrderBy(u => u.TUserID).FirstOrDefault();
                    if (firstUser != null)
                    {
                        return firstUser.TUserID;
                    }
                }
                var newUser = new TUser { Name = userName, LastTaskID = 0, CurrentState = TState.Idle };
                _db.TUsers.Add(newUser);
                _db.SaveChanges();

                var newProject = new TProject
                {
                    DisplayOrder = 1,
                    IsObsolete = false,
                    Name = "Sample Project",
                    TUserID = newUser.TUserID
                };
                _db.TProjects.Add(newProject);
                _db.SaveChanges();

                var newTask = new TTask
                {
                    DisplayOrder = 1,
                    IsObsolete = false,
                    Name = "Sample Task",
                    TProjectID = newProject.TProjectID
                };
                _db.TTasks.Add(newTask);
                _db.SaveChanges();

                return newUser.TUserID;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TSummaryModelDto GetSummaryModel(TContextDto context)
        {
            try
            {
                using (var calc = new TracktorCalculator(context))
                {
                    return calc.BuildSummaryModel(null, calc.DateOrLocalNow(null));
                }
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TStatusModelDto GetStatusModel(TContextDto context)
        {
            try
            {
                using (var calc = new TracktorCalculator(context))
                {
                    return calc.BuildStatusModel();
                }
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TEntriesModelDto GetEntriesModel(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID, int startNo, int maxEntries)
        {
            try
            {
                using (var calc = new TracktorCalculator(context))
                {
                    var entries = calc.GetEntries(startDate, calc.DateOrLocalNow(endDate), projectID, startNo, maxEntries);
                    return new TEntriesModelDto
                    {
                        Entries = calc.CalculateEntryContribs(entries, startDate, calc.DateOrLocalNow(endDate))
                    };
                }
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
                return UpdateObject<TTaskDto, TTask>(context, task, _db.TTasks, (t => t.TTaskID == task.TTaskID));
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
                return UpdateObject<TProjectDto, TProject>(context, project, _db.TProjects, (t => t.TProjectID == project.TProjectID));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TEntryDto GetEntry(TContextDto context, int entryID)
        {
            try
            {
                using (var calc = new TracktorCalculator(context))
                {
                    var entry = _db.TEntries.SingleOrDefault(e => e.TEntryID == entryID && e.TTask.TProject.TUserID == context.TUserID);
                    return calc.EnrichTEntry(null, entry);
                }
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
                if (entry.TEntryID > 0)
                {
                    var existingEntry = _db.TEntries.SingleOrDefault(e => e.TEntryID == entry.TEntryID);
                    if (existingEntry != null)
                    {
                        using (var calculator = new TracktorCalculator(context))
                        {
                            if (existingEntry.TTask.TProject.TUserID == context.TUserID)
                            {
                                if (entry.IsDeleted == true)
                                {
                                    // stop if in progress
                                    if (!existingEntry.EndDate.HasValue)
                                    {
                                        StopTask(context, entry.TTaskID);
                                        existingEntry = _db.TEntries.SingleOrDefault(e => e.TEntryID == entry.TEntryID);
                                    }
                                    _db.TEntries.Remove(existingEntry);
                                    _db.SaveChanges();
                                    return null;
                                }
                                else
                                {
                                    var startUtc = calculator.ToUtc(entry.StartDate).Value;
                                    var endUtc = calculator.ToUtc(entry.EndDate);
                                    var now = DateTime.UtcNow;
                                    if (startUtc <= now &&
                                        (!endUtc.HasValue || (endUtc.HasValue && endUtc.Value <= now && endUtc.Value > startUtc)))
                                    {
                                        existingEntry.StartDate = startUtc;
                                        existingEntry.EndDate = endUtc;
                                        _db.SaveChanges();
                                    }
                                }
                            }
                            return calculator.EnrichTEntry(null, existingEntry);
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public TReportModelDto GetReportModel(TContextDto context, DateTime? startDate, DateTime? endDate, int projectID)
        {
            try
            {
                using (var calculator = new TracktorCalculator(context))
                {
                    return Mapper.Map<TReportModelDto>(calculator.GetReport(startDate, calculator.DateOrLocalNow(endDate), projectID));
                }
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public void StopTask(TContextDto context, int currentTaskID)
        {
            try
            {
                using (var states = new TracktorStates(context))
                {
                    states.Stop(currentTaskID);
                }
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public void StartTask(TContextDto context, int newTaskID)
        {
            try
            {
                using (var states = new TracktorStates(context))
                {
                    states.Start(newTaskID);
                }
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        public void SwitchTask(TContextDto context, int currentTaskID, int newTaskID)
        {
            try
            {
                using (var states = new TracktorStates(context))
                {
                    states.Stop(currentTaskID);
                    states.Start(newTaskID);
                }
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region Helpers

        protected D UpdateObject<D, T>(TContextDto context, D dto, DbSet<T> dbSet, Expression<Func<T, bool>> equality)
            where D : new()
            where T : class, new()
        {
            int dtoId = 0;
            var storedObject = dbSet.Where(Expression.Lambda<Func<T, bool>>(equality, Expression.Parameter(typeof(T), "t"))).SingleOrDefault();
            if (storedObject == null)
            {
                if (dtoId != 0)
                {
                    throw new Exception(String.Format("Unable to find existing {0} object with ID {1}!", typeof(T).Name, dtoId));
                }
                storedObject = new T();
                dbSet.Add(storedObject);
            }
            else if (!IsAllowed(context, storedObject))
            {
                throw new Exception(String.Format("User ID {0} is not allowed to modify {1} object with ID {2}!", context.TUserID, typeof(T).Name, dtoId));
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
