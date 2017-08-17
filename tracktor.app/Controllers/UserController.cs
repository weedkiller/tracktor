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
    [Authorize]
    [Produces("application/json")]
    [Route("api/user")]
    public class UserController : TracktorControllerBase
    {
        public UserController(ITracktorService service, UserManager<ApplicationUser> userManager) : base(service, userManager)
        {
        }

        [HttpPost("update")]
        public async Task<TracktorWebModel> Update([FromBody]LoginDTO login)
        {
            var userId = _userManager.GetUserId(Request.HttpContext.User);
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user != null && login != null)
            {
                user.TimeZone = login.timeZone;
                await _userManager.UpdateAsync(user);
            }

            return await GenerateWebModel();
        }
    }
}
