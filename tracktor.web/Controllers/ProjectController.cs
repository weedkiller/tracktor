﻿using System;
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
    public class ProjectController : TracktorControllerBase
    {
        public ProjectController(ITracktorService service)
            : base(service)
        {
        }

        [Route("project/update")]
        [HttpPost]
        public TProjectDto Update(TProjectDto project)
        {
            return _service.UpdateProject(Context, project);
        }
    }
}
