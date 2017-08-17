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
using tracktor.app.Models;

namespace tracktor.app.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/entry")]
    public class EntryController : TracktorController
    {
        public EntryController(ITracktorService service, UserManager<ApplicationUser> userManager) : base(service, userManager)
        {
        }

        [HttpPost("update")]
        public async Task<TracktorWebModel> Update([FromBody]TEntryDto entry)
        {
            return new TracktorWebModel
            {
                EditModel = new TEditModelDto
                {
                    Entry = await _service.UpdateEntryAsync(Context, entry)
                }
            };
        }

        [HttpGet("{entryID}")]
        public async Task<TracktorWebModel> Get([FromRoute]int entryID)
        {
            return new TracktorWebModel
            {
                EditModel = new TEditModelDto
                {
                    Entry = await _service.GetEntryAsync(Context, entryID)
                }
            };
        }
    }
}
