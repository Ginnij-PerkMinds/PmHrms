using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace PmHrmsAPI.PmHrmsBAL.Services
{
    public class EmailService : IEmailService
    {

        private readonly PmHrmsContext _context;

        public EmailService(PmHrmsContext context)
            {
              _context = context;
            }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {

            var settings = await _context.SystemMailSettings.FirstOrDefaultAsync();      

            if (settings == null)            
            throw new Exception("System mail settings not configured.");                   

            var email = new MimeMessage();                                  

            email.Sender = MailboxAddress.Parse(settings.Mail);                                                                             
             email.From.Add(new MailboxAddress(settings.DisplayName, settings.Mail));                                                       
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));             
            email.Subject = mailRequest.Subject;      

            var builder = new BodyBuilder { HtmlBody = mailRequest.Body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                
               await smtp.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.StartTls);

            string loginUser = !string.IsNullOrEmpty(settings.UserName)
                                ? settings.UserName
                                : settings.Mail;

            await smtp.AuthenticateAsync(loginUser, settings.Password);
            await smtp.SendAsync(email);
                
            }
            catch (Exception ex)
            {
                
                throw new Exception($"Email sending failed via {settings.Host}: {ex.Message}");
           }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}