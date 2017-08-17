using Newtonsoft.Json;

namespace tracktor.app
{
    public class AccountDTO
    {
        public string email;
        public string password;
        public string newPassword;
        public string code;
        public string provider;
        public string timezone;
        public string[] messages;
        public bool remember;

        [JsonIgnore]
        public string Username => email != null ? email.Trim().ToLower() : null;
    }
}