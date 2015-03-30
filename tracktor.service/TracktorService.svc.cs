// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

using AutoMapper;
using log4net;
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
    public class TracktorService : ITracktorService
    {
        private ITracktorContext _db;
        private ILog _log;

        public TracktorService(ITracktorContext db, ILog log)
        {
            _db = db;
            _log = log;
        }

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
                var calc = new TracktorCalculator(context, _db);
                return calc.BuildSummaryModel(null, calc.DateOrLocalNow(null));
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
                var calc = new TracktorCalculator(context, _db);
                return calc.BuildStatusModel();
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
                var calc = new TracktorCalculator(context, _db);
                var entries = calc.GetEntries(startDate, calc.DateOrLocalNow(endDate), projectID, startNo, maxEntries);
                return new TEntriesModelDto
                {
                    Entries = calc.CalculateEntryContribs(entries, startDate, calc.DateOrLocalNow(endDate))
                };
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
                if (task.TTaskID > 0)
                {
                    var existingTask = _db.TTasks.SingleOrDefault(e => e.TTaskID == task.TTaskID);
                    if (existingTask != null)
                    {
                        if (existingTask.TProject.TUserID == context.TUserID)
                        {
                            // if there are no entries, remove it instead
                            if (task.IsObsolete && !_db.TEntries.Any(e => e.TTaskID == task.TTaskID))
                            {
                                _db.TTasks.Remove(existingTask);
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(task.Name))
                                {
                                    existingTask.Name = task.Name;
                                }
                                existingTask.IsObsolete = task.IsObsolete;
                                existingTask.DisplayOrder = task.DisplayOrder;
                            }
                            _db.SaveChanges();
                        }
                        return Mapper.Map<TTaskDto>(existingTask);
                    }
                }
                else if (task.TProjectID > 0 && !task.IsObsolete && !string.IsNullOrWhiteSpace(task.Name))
                {
                    var existingProject = _db.TProjects.SingleOrDefault(p => p.TProjectID == task.TProjectID && p.TUserID == context.TUserID);
                    if (existingProject != null)
                    {
                        var newTask = new TTask
                        {
                            DisplayOrder = task.DisplayOrder,
                            IsObsolete = task.IsObsolete,
                            Name = task.Name,
                            TProjectID = task.TProjectID
                        };
                        _db.TTasks.Add(newTask);
                        _db.SaveChanges();
                        return Mapper.Map<TTaskDto>(newTask);
                    }
                }
                return null;
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
                if (project.TProjectID > 0)
                {
                    var existingProject = _db.TProjects.SingleOrDefault(e => e.TProjectID == project.TProjectID);
                    if (existingProject != null)
                    {
                        if (existingProject.TUserID == context.TUserID)
                        {
                            // if there are no tasks, remove it instead
                            if (project.IsObsolete && !_db.TTasks.Any(e => e.TProjectID == project.TProjectID))
                            {
                                _db.TProjects.Remove(existingProject);
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(project.Name))
                                {
                                    existingProject.Name = project.Name;
                                }
                                existingProject.IsObsolete = project.IsObsolete;
                                existingProject.DisplayOrder = project.DisplayOrder;
                            }
                            _db.SaveChanges();
                        }
                        return Mapper.Map<TProjectDto>(existingProject);
                    }
                }
                else if (!project.IsObsolete && !string.IsNullOrWhiteSpace(project.Name))
                {
                    var newProject = new TProject
                    {
                        DisplayOrder = project.DisplayOrder,
                        IsObsolete = project.IsObsolete,
                        Name = project.Name,
                        TUserID = context.TUserID
                    };
                    _db.TProjects.Add(newProject);
                    _db.SaveChanges();
                    return Mapper.Map<TProjectDto>(newProject);
                }
                return null;
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
                var calc = new TracktorCalculator(context, _db);
                var entry = _db.TEntries.Single(e => e.TEntryID == entryID && e.TTask.TProject.TUserID == context.TUserID);
                return calc.EnrichTEntry(null, entry);
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
                        var calculator = new TracktorCalculator(context, _db);
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
                var calculator = new TracktorCalculator(context, _db);
                return Mapper.Map<TReportModelDto>(calculator.GetReport(startDate, calculator.DateOrLocalNow(endDate), projectID));
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
                var states = new TracktorStates(context, _db);
                states.Stop(currentTaskID);
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
                var startTask = _db.TTasks.Single(t => t.TTaskID == newTaskID);
                if (startTask.TProject.TUserID != context.TUserID)
                {
                    throw new Exception("Invalid Task ID.");
                }
                var states = new TracktorStates(context, _db);
                states.Start(newTaskID);
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
                var startTask = _db.TTasks.Single(t => t.TTaskID == newTaskID);
                if (startTask.TProject.TUserID != context.TUserID)
                {
                    throw new Exception("Invalid Task ID.");
                }
                var states = new TracktorStates(context, _db);
                states.Stop(currentTaskID);
                states.Start(newTaskID);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<string>(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
