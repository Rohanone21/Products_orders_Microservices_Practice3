using System.Net;
using System.Net.Mail;

namespace Orders_New_MVC_Microservice.Models
{
    public class EmailHelper
    {
        private readonly string FromEmail = "lukas.heller@ethereal.email";
        private readonly string Password = "h2dVwAxQSm9vGMfJmj";

        public bool Send(string toEmail, string subject, string message)
        {
            try
            {
                var smtp = new SmtpClient("smtp.ethereal.email")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(FromEmail, Password),
                    EnableSsl = true
                };

                var mail = new MailMessage(FromEmail, toEmail)
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                smtp.Send(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
