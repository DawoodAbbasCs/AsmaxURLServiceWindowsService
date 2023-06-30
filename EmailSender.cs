using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using Serilog;
using Serilog.Core;
using System.Configuration;

namespace WindowsService1
{
    public static class EmailSender
    {
        private static ILogger logger;
        private static string smtpServer;
        private static int smtpPort;
        private static string smtpUsername;
        private static string smtpPassword;
        static EmailSettingsSection emailSettings;

        
        public static void SendEmail(string toAddress, string subject, string body)
        {
            try
            {
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                string logFile = Path.Combine(logDirectory, $"log_{DateTime.Now.ToString("yyyyMMdd")}.txt");

                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File(logFile)
                    .CreateLogger();

                logger = Log.Logger;

                emailSettings = ConfigurationManager.GetSection("emailSettings") as EmailSettingsSection;

                smtpServer = emailSettings.SmtpSettings.Server;
                smtpPort = emailSettings.SmtpSettings.Port;
                smtpUsername = emailSettings.SmtpSettings.Email;
                smtpPassword = emailSettings.SmtpSettings.Password;


                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtpUsername);
                mail.To.Add(toAddress);
                mail.Subject = subject;
                mail.Body = body;

                SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                logger.Information($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} Error sending email: {ex.Message}");
            }
        }
    }
}
