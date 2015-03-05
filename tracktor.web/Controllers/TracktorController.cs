using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using tracktor.service;

namespace tracktor.web.Controllers
{
    [Authorize]
    public class TracktorController : ApiController
    {
        private ITracktorService _service = new TracktorService(); // could also be remotely hosted

        private TContextDto Context
        {
            get
            {
                return new TContextDto {
                    TUserID = Int32.Parse(this.Request.GetOwinContext().Authentication.User.FindFirst("TUserID").Value),
                    UTCOffset = 5
                };
            }
        }

        public TModelDto GetModel()
        {
            return _service.GetModel(Context);
        }

        public TTaskDto UpdateTask(TTaskDto task)
        {
            return _service.UpdateTask(Context, task);
        }

        public TProjectDto UpdateProject(TProjectDto project)
        {
            return _service.UpdateProject(Context, project);
        }

        public TEntryDto UpdateEntry(TEntryDto entry)
        {
            return _service.UpdateEntry(Context, entry);
        }

        public List<TEntryDto> GetEntries(DateTime? startDate, DateTime? endDate, int projectID, int maxEntries)
        {
            return _service.GetEntries(Context, startDate, endDate, projectID, maxEntries);
        }

        public TracktorReportDto GetReport(DateTime? startDate, DateTime? endDate, int projectID)
        {
            return _service.GetReport(Context, startDate, endDate, projectID);
        }

        public TModelDto StopTask(int currentTaskID)
        {
            return _service.StopTask(Context, currentTaskID);
        }

        public TModelDto StartTask(int newTaskID)
        {
            return _service.StartTask(Context, newTaskID);
        }

        public TModelDto SwitchTask(int currentTaskID, int newTaskId)
        {
            return _service.SwitchTask(Context, currentTaskID, newTaskId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_service != null && _service is IDisposable)
                {
                    (_service as IDisposable).Dispose();
                    _service = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
