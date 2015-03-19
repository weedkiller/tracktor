// copyright (c) 2015 rohatsu software studios limited (www.rohatsu.com)
// licensed under the apache license, version 2.0; see LICENSE for details
// 

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
    [Authorize]
    public class EntryController : TracktorControllerBase
    {
        public EntryController(ITracktorService service) : base(service)
        {
        }

        [Route("entry/update")]
        [HttpPost]
        public TracktorWebModel Update(TEntryDto entry)
        {
            return new TracktorWebModel
            {
                EditModel = new TEditModelDto
                {
                    Entry = _service.UpdateEntry(Context, entry)
                }
            };
        }

        [Route("entry/{entryID}")]
        [HttpGet]
        public TracktorWebModel Get(int entryID)
        {
            return new TracktorWebModel
            {
                EditModel = new TEditModelDto
                {
                    Entry = _service.GetEntry(Context, entryID)
                }
            };
        }
    }
}
