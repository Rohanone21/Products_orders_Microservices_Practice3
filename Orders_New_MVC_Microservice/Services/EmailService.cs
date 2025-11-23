using System.Net;
using System.Net.Mail;

namespace Orders_New_MVC_Microservice.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.ethereal.email")
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    _config["Email:Username"],
                    _config["Email:Password"]
                ),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:Username"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            await smtpClient.SendMailAsync(mail);
        }
    }

}
