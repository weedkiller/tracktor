using System.Net.Mail;
using System.Threading.Tasks;

namespace tracktor.app
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message, string link);
    }

    public class EmailSender : IEmailSender
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _useSsl;
        private readonly string _sender;
        private readonly string _username;
        private readonly string _password;

        public EmailSender(string host, int port, bool useSsl, string sender, string username, string password)
        {
            _host = host;
            _port = port;
            _sender = sender;
            _useSsl = useSsl;
            _username = username;
            _password = password;
        }

        public async Task SendEmailAsync(string email, string subject, string message, string link)
        {
            var s = new SmtpClient();

            var mm = new MailMessage();
            mm.From = new MailAddress(_sender);
            mm.To.Add(new MailAddress(email));
            mm.Subject = subject;
            mm.IsBodyHtml = true;
            mm.Body = $"<p>{message}</p><p><a href='{link}'>{link}</p>";

            using (var client = new SmtpClient(_host, _port))
            {
                client.EnableSsl = _useSsl;
                if (!string.IsNullOrWhiteSpace(_username))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(_username, _password);
                }
                await client.SendMailAsync(mm);
            }
        }
    }
}
