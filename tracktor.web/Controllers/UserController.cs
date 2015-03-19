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
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace tracktor.web.Controllers
{
    [Authorize]
    public class UserController : TracktorControllerBase
    {
        public UserController(ITracktorService service) : base(service)
        {
        }

        [Route("user/update")]
        [HttpPost]
        public TracktorWebModel Update(ManageInfoViewModel userModel)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = userManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                user.TimeZone = userModel.TimeZone;
                userManager.Update(user);
            }

            return GenerateWebModel();
        }
    }
}
