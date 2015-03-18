using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security;
using System.Security.Authentication;
using System.Web;

namespace tracktor.service
{
    public class TracktorServiceValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (!string.Equals(password, ConfigurationManager.AppSettings["ServicePassword"]))
            {
                throw new AuthenticationException("Incorrect service access credentials.");
            }
        }
    }
}