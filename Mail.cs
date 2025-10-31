using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace consoleSAMOIG
{
    internal class Mail
    {
        public static void MyMail()
        {

            // Configure SMTP client
            using (var client = new SmtpClient("smtp.office365.com", 587))
            {
                client.Credentials = new NetworkCredential(Globals.conSmtpSender, Globals.conSmtpPassword);
                client.EnableSsl = true;

                // Create email message
                var message = new MailMessage(Globals.conSmtpSender, Globals.conNotifyOnError)
                {
                    Subject = "Test Email from C#",
                    Body = "This is a test email sent using Office 365 SMTP.",
                    IsBodyHtml = false // Set to true if sending HTML content
                };

                try
                {
                    // Send the email
                    client.Send(message);
                    Console.WriteLine("Email sent successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                }
            }
        }

    }
}
