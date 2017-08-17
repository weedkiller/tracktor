using System.Collections.Generic;

namespace tracktor.app
{
    public class LoginDTO
    {
        public string id;
        public string email;
        public IEnumerable<string> roles;
        public string afToken;
        public string afHeader;
        public string timeZone;
    }
}