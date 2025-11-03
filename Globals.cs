using Microsoft.Extensions.Configuration;

namespace consoleSAMOIG
{
    public static class Globals
    {
        private static IConfiguration? _configuration;

        public static void Initialize()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();
        }

        internal static string conString => _configuration?["ConnectionStrings:DefaultConnection"] ?? throw new InvalidOperationException("Connection string not configured");
        internal static string conSamApiKey => _configuration?["ApiKeys:SAM"] ?? throw new InvalidOperationException("SAM API key not configured");
        internal static string conSendGrid => _configuration?["SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid API key not configured");
        internal static string conEmailFrom => _configuration?["Email:FromAddress"] ?? "Admin@CPNW.org";
        internal static string conEmailFromName => _configuration?["Email:FromName"] ?? "Admin";
        internal static string conReportToEmail => _configuration?["Email:ReportToEmail"] ?? throw new InvalidOperationException("Report email not configured");
        internal static string conSmtpSender => _configuration?["Smtp:Sender"] ?? throw new InvalidOperationException("SMTP sender not configured");
        internal static string conSmtpPassword => _configuration?["Smtp:Password"] ?? throw new InvalidOperationException("SMTP password not configured");
        internal static string conNotifyOnError => _configuration?["Email:NotifyOnError"] ?? throw new InvalidOperationException("Error notification address not configured");
        internal static string GetJulianDate(DateTime dtm2Convert)
        {
            string strDOY = "0" + dtm2Convert.DayOfYear.ToString();
            string strYY = dtm2Convert.ToString("yy");
            return strYY + Right(strDOY, 3);
        }

        internal static string Right(string sValue, int iMaxLength)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(sValue))
            {
                //Set valid empty string as string could be null
                sValue = string.Empty;
            }
            else if (sValue.Length > iMaxLength)
            {
                //Make the string no longer than the max length
                sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);
            }

            return sValue;
        }
    }
}
