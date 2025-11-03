using SendGrid;
using SendGrid.Helpers.Mail;

namespace consoleSAMOIG
{

    public class EmailService
    {
        private readonly string _sendGridApiKey;

        public EmailService(string sendGridApiKey)
        {
            _sendGridApiKey = sendGridApiKey;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress(Globals.conEmailFrom, Globals.conEmailFromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            Console.WriteLine($"SendGrid - Sending email:");
            Console.WriteLine($"  From: {from.Email} ({from.Name})");
            Console.WriteLine($"  To: {to.Email}");
            Console.WriteLine($"  Subject: {subject}");

            var response = await client.SendEmailAsync(msg);

            Console.WriteLine($"SendGrid Response Status: {response.StatusCode} ({(int)response.StatusCode})");
            Console.WriteLine($"SendGrid Response Headers:");
            foreach (var header in response.Headers)
            {
                Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }

            var body = await response.Body.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(body))
            {
                Console.WriteLine($"SendGrid Response Body: {body}");
            }

            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }

}
