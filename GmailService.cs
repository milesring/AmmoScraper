using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AmmoScraper
{
    public class GmailService
    {
        private SmtpClient SmtpClient { get; set; }
        public bool Configured { get; set; }
        public GmailService()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            SmtpClient = new SmtpClient("smtp.gmail.com");
            SmtpClient.EnableSsl = true;
            SmtpClient.Port = 587;

            string file;
            try
            {
                file = File.ReadAllText(@".\EmailConfig");
            }
            catch(FileNotFoundException e)
            {
                Console.WriteLine("{0} - EMAIL CONFIG NOT FOUND, PLACEHOLDER INFO ADDED TO EMAILCONFIG FILE AT ROOT", DateTime.Now);
                var credentials = new NetworkCredential();
                credentials.UserName = "UserName";
                credentials.Password = "GmailAppPassword";

                var json = JsonConvert.SerializeObject(credentials, Formatting.Indented);
                File.WriteAllText(@".\EmailConfig", json);
                Configured = false;
                return;
            }

            var creds = JsonConvert.DeserializeObject<NetworkCredential>(file);
            if(creds.UserName.Contains("UserName", StringComparison.OrdinalIgnoreCase) || 
                creds.Password.Contains("GmailAppPasswrd", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("{0} - PLACEHOLDER INFORMATION FOUND FOR EMAIL SERVICE, PLEASE CONFIGURE EMAILCONFIG FILE AT ROOT", DateTime.Now);
                Configured = false;
                return;
            }
            SmtpClient.Credentials = creds;
            Configured = true;
        }

        public void SendEmail(string m)
        {
            string subject = "TargetSportsUSA in stock ammo alert - AmmoScraper";
            string message = "In stock ammo at TargetSportsUSA!\n\n";
            message += m;

            var mailMessage = new MailMessage(
                "ring.miles@gmail.com",
                "ring.miles@gmail.com",
                subject,
                message
                );
            mailMessage.Priority = MailPriority.High;
            SmtpClient.Send(mailMessage);
        }

    }

   


}
