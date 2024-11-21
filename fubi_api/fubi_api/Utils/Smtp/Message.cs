using fubi_api.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace fubi_api.Utils.Smtp
{
    public class Message : IMessage
    {
        public GmailSettings _gmailSettings { get;}

        public Message(IOptions<GmailSettings> gmailSettings)
        {
            _gmailSettings = gmailSettings.Value;
        }

        public void SendEmail(string subject, string body, string to)
        {
            try
            {
                var fromEmail = _gmailSettings.Username;
                var password = _gmailSettings.Password;

                var message = new MailMessage();
                message.From = new MailAddress(fromEmail!);
                message.Subject = subject;
                message.To.Add(new MailAddress(to));
                message.Body = body;
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = _gmailSettings.Port,
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = true
                };

                smtpClient.Send(message);


            }catch (Exception ex) {
                throw new Exception("No se puede enviar el email", ex);
        }
    }
}
}

