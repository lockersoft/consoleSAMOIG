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
            var from = new EmailAddress(Globals.conEmailFrom, Globals.conEmailFrom);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }

}
