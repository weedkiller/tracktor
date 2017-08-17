// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
//

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using tracktor.app;
using tracktor.app.Models;

namespace tracktor.app.Controllers
{
    public class TracktorEntryAction
    {
        public int currentTaskID { get; set; }
        public int newTaskID { get; set; }
    }

    [Authorize]
    [Produces("application/json")]
    [Route("api/task")]
    public class TaskController : TracktorControllerBase
    {
        public TaskController(ITracktorService service, UserManager<ApplicationUser> userManager) : base(service, userManager)
        {
        }

        [HttpPost("update")]
        public async Task<TTaskDto> Update([FromBody]TTaskDto task)
        {
            return await _service.UpdateTaskAsync(Context, task);
        }

        [HttpPost("stop")]
        public async Task<TracktorWebModel> Stop([FromBody]TracktorEntryAction actionModel)
        {
            await _service.StopTaskAsync(Context, actionModel.currentTaskID);
            return await GenerateWebModel(true);
        }

        [HttpPost("start")]
        public async Task<TracktorWebModel> Start([FromBody]TracktorEntryAction actionModel)
        {
            await _service.StartTaskAsync(Context, actionModel.newTaskID);
            return await GenerateWebModel(true);
        }

        [HttpPost("switch")]
        public async Task<TracktorWebModel> Switch([FromBody]TracktorEntryAction actionModel)
        {
            await _service.SwitchTaskAsync(Context, actionModel.currentTaskID, actionModel.newTaskID);
            return await GenerateWebModel(true);
        }
    }
}
