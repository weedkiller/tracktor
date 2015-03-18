using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.Owin;
using tracktor.service;
using tracktor.web.Models;

namespace tracktor.web.Controllers
{
    public class TracktorEntryAction
    {
        public int currentTaskID { get; set; }
        public int newTaskID { get; set; }
    }

    [Authorize]
    public class TaskController : TracktorControllerBase
    {
        public TaskController(ITracktorService service) : base(service)
        {
        }

        [Route("task/update")]
        [HttpPost]
        public TTaskDto Update(TTaskDto task)
        {
            return _service.UpdateTask(Context, task);
        }

        [Route("task/stop")]
        [HttpPost]
        public TracktorWebModel Stop(TracktorEntryAction actionModel)
        {
            _service.StopTask(Context, actionModel.currentTaskID);
            return GenerateWebModel(true);
        }

        [Route("task/start")]
        [HttpPost]
        public TracktorWebModel Start(TracktorEntryAction actionModel)
        {
            _service.StartTask(Context, actionModel.newTaskID);
            return GenerateWebModel(true);
        }

        [Route("task/switch")]
        [HttpPost]
        public TracktorWebModel Switch(TracktorEntryAction actionModel)
        {
            _service.SwitchTask(Context, actionModel.currentTaskID, actionModel.newTaskID);
            return GenerateWebModel(true);
        }
    }
}
