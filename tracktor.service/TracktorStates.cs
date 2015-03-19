// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tracktor.model;
using tracktor.model.DAL;

namespace tracktor.service
{
    internal class TracktorStates
    {
        protected TContextDto mContext;
        protected StateMachine<TState, TTrigger> mStateMachine;

        private ITracktorContext _db;
        private StateMachine<TState, TTrigger>.TriggerWithParameters<int> _startTrigger;
        private StateMachine<TState, TTrigger>.TriggerWithParameters<int> _stopTrigger;

        public TracktorStates(TContextDto context, ITracktorContext db)
        {
            _db = db;
            mContext = context;
            mStateMachine = new StateMachine<TState, TTrigger>(GetCurrentState, ChangeState);
            _startTrigger = mStateMachine.SetTriggerParameters<int>(TTrigger.Start);
            _stopTrigger = mStateMachine.SetTriggerParameters<int>(TTrigger.Stop);

            mStateMachine.Configure(TState.Idle)
                .PermitDynamic(_startTrigger, newTaskId => AcceptStart(newTaskId))
                .OnEntryFrom(_stopTrigger, newTaskId => StopTask(newTaskId));
            mStateMachine.Configure(TState.InProgress)
                .PermitDynamic(_stopTrigger, newTaskId => AcceptStop(newTaskId))
                .OnEntryFrom(_startTrigger, newTaskId => StartTask(newTaskId));
        }

        protected TState AcceptStart(int newTaskId)
        {
            if (newTaskId > 0)
            {
                return TState.InProgress;
            }
            else
            {
                throw new Exception(String.Format("Not allowed to start task {0} now.", newTaskId));
            }
        }

        protected TState AcceptStop(int currentTaskId)
        {
            var storedTaskId = GetCurrentTask();
            if (currentTaskId == storedTaskId)
            {
                return TState.Idle;
            }
            else
            {
                throw new Exception(String.Format("Can't stop task {0} while {1} is in progress!", currentTaskId, storedTaskId));
            }
        }

        public void Start(int newTaskId)
        {
            mStateMachine.Fire(_startTrigger, newTaskId);
            _db.SaveChanges();
        }

        public void Stop(int currentTaskId)
        {
            mStateMachine.Fire(_stopTrigger, currentTaskId);
            _db.SaveChanges();
        }

        protected TState GetCurrentState()
        {
            var user = _db.TUsers.Where(u => u.TUserID == mContext.TUserID).FirstOrDefault();
            if (user != null)
            {
                return user.CurrentState;
            }
            throw new Exception(String.Format("Unable to retrieve state for user {0}!", mContext.TUserID));
        }

        protected void ChangeState(TState newState)
        {
            var user = _db.TUsers.Where(u => u.TUserID == mContext.TUserID).FirstOrDefault();
            if (user != null)
            {
                user.CurrentState = newState;
            }
        }

        protected int GetCurrentTask()
        {
            var user = _db.TUsers.Where(u => u.TUserID == mContext.TUserID).FirstOrDefault();
            if (user != null)
            {
                return user.LastTaskID;
            }
            throw new Exception(String.Format("Unable to retrieve last task for user {0}!", mContext.TUserID));
        }

        protected void StartTask(int newTaskId)
        {
            try
            {
                var user = _db.TUsers.Where(u => u.TUserID == mContext.TUserID).Single();
                user.LastTaskID = newTaskId;
                _db.TEntries.Add(new TEntry
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = null,
                    TTaskID = newTaskId
                });
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error starting task {0} for user {1}: {2}", newTaskId, mContext.TUserID, ex.Message));
            }
        }

        protected void StopTask(int currentTaskId)
        {
            try
            {
                var user = _db.TUsers.Where(u => u.TUserID == mContext.TUserID).Single();
                user.LastTaskID = currentTaskId;
                var entry = _db.TEntries.Where(e => e.TTaskID == currentTaskId && !e.EndDate.HasValue).Single();
                entry.EndDate = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error stopping task {0} for user {1}: {2}", currentTaskId, mContext.TUserID, ex.Message));
            }
        }
    }
}
