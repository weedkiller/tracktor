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
    public class TracktorController : TracktorControllerBase
    {
        public TracktorController(ITracktorService service) : base(service)
        {
        }

        [Route("viewmodel")]
        [HttpGet]
        public TracktorWebModel GetViewModel(bool updateOnly = false)
        {
            return GenerateWebModel(updateOnly);
        }
    }
}
