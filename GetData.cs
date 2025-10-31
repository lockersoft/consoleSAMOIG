using consoleSAMOIG.Models;
using System.Globalization;
using System.IO.Compression;


namespace consoleSAMOIG
{
    public static class GetData
    {
        //Note: there is a Person class at the bottom of this file

        //Saves data to users temp files
        private static readonly System.String filePath = Path.GetTempPath();
        private static readonly HttpClient _httpClient = new();    

       public static async Task BuildOIG()
        {
            //Path to OIG 
            var fileUrl = @"https://oig.hhs.gov/exclusions/downloadables/UPDATED.csv";

            //Path to local file to save download.  Uses temp path
            var localPath = $@"{filePath}updatedleie.csv";

            //My error was using DateOnly fields in Contacts.  I need to pull time out of the comparison
            var myDateOnly = DateOnly.FromDateTime(DateTime.Now);

            //Create the database context
            var context = new SAMOIGdat();

            //Download the file
            using HttpClient client = new();
            try
            {
                //Set a user-agent header to avoid 403 Forbidden errors
                client.DefaultRequestHeaders.Add("user-agent", "Doing BG Check for CPNW");
                //Get the file stream from the URL
                using (Stream stream = await client.GetStreamAsync(fileUrl))
                {
                    // Save the CSV file to the local path
                    using FileStream fileStream = new(localPath, FileMode.Create, FileAccess.Write);
                    await stream.CopyToAsync(fileStream);
                }

                //Read all rows into the local path into lis GoodOIG.  Exclude those with bad birthdates and empty.
                //I convert the string Birthdate into a DateTime

                //I created a Person class to hold the name and birthdate of names in the OIG CSV file
                var GoodOIG = File.ReadAllLines(localPath)
                    .Skip(1) // skip header
                    .Select(line => line.Split(','))
                    .Where(fields =>
                        fields[0].Trim('"').Trim() != "" && //Last Name not empty after trimming quotes and spaces
                                                            //Check for valid date in YYYYMMDD format
                        DateTime.TryParseExact(fields[8].Trim('"'), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    )
                    .Select(fields =>
                    {
                        //Parse the birthdate string into a DateOnly
                        DateOnly birthDate = DateOnly.ParseExact(fields[8].Trim('"'), "yyyyMMdd", CultureInfo.InvariantCulture);

                        return new Person
                        {
                            First = fields[1].Trim('"'),
                            Last = fields[0].Trim('"'),
                            BirthDate = birthDate 
                        };
                    })
                    .ToList();


                //Join GoodOIG with Contacts on First, Last, and BirthDate
                //Result is in matchedContacts and is a list of Contacts failing OIG
                var matchedContacts = (from person in GoodOIG
                                       join contact in context.Contacts
                                       on new
                                       {
                                           NameFirst = person.First?.Trim().ToUpper(),
                                           NameLast = person.Last?.Trim().ToUpper(),
                                           BirthDate = person.BirthDate.ToString()

                                       }
                                       equals new
                                       {
                                           NameFirst = contact.NameFirst?.Trim().ToUpper(),
                                           NameLast = contact.NameLast?.Trim().ToUpper(),
                                           BirthDate = contact.Dob.ToString()
                                       }
                                       where (contact.TypeContactIdfk == 27 || contact.TypeContactIdfk == 36)
                                             && contact.RegistrationStatus == "Approved"
                                             && contact.Archived == null
                                             && contact.Ssn != null
                                             && contact.Ssn.Length == 11
                                             && contact.Ssn.Substring(0, 3) != "000"
                                             && contact.Ssn.Substring(0, 3) != "666"
                                       select contact).ToList();

                //Update ExclusionHits for each matched contact
                foreach (var contact in matchedContacts)
                {
                    var hitsToUpdate = context.ExclusionHits
                    .Where(e => e.ContactIdfk == contact.ContactIdpk)
                    .ToList();
                    //Only update today's records that are OIG - Here you change the 'Pass' to 'Fail'
                    foreach (var hit in hitsToUpdate.Where(h => h.DateRun == myDateOnly && h.TableHit == "OIG"))
                    {
                        hit.NameHit = "OIG";
                        hit.TableHit = "OIG";
                        hit.LikelyMatch = "Fail";
                    }
                    context.SaveChanges();
                }
                await SendEmail(Globals.conReportToEmail, "Success OIG", "OIG Exclusion Records Created", "<strong>OIG Exclusion Records Created</strong>");
            }

            catch (HttpRequestException e)
            {
                await SendEmail(Globals.conReportToEmail, $"BuildOIG() HTTP Request Error {e.Message}", $"Error: {e.Message}", $"<strong>Error:{e.Message}</strong>");
            }

            catch (Exception ex)
            {
                await SendEmail(Globals.conReportToEmail, "BuildOIG() Error", $"Error: {ex.Message}", $"<strong>Error:{ex.Message}</strong>");
            }
        }

        public async static Task BuildSAM()
        {
            //
            DateOnly myDateOnly = DateOnly.FromDateTime(DateTime.Now);
            DateTime myDate= DateTime.Now;
            string localFilePath = Path.Combine(filePath, "Download.zip");

            //First get the SAM API key from configuration
            var context = new SAMOIGdat();
            bool found = false;  //forces first loop iteration
            int intBackDay = 0;  //Number of days to backup to find latest SAM file
            while (!found)
            {
                myDate = DateTime.Now.AddDays(intBackDay);  //Date to backup to
                myDateOnly = DateOnly.FromDateTime(myDate);

                // Check if we've tried too many times
                if (intBackDay <= -10)
                {
                    await SendEmail(
                        Globals.conReportToEmail,
                        "BuildSAM() Error",
                        "Too Many SAM Download attempts - expired bad key",
                        "<strong>Too Many SAM Download attempts - likely expired key</strong>"
                    );
                    break;
                }

                try
                {
                    string myJul = Globals.GetJulianDate(myDate);  //returns Julian Date (2 digit year + day of year) e.g. 25123

                    // Check if file already exists (for testing/debugging)
                    if (File.Exists(localFilePath))
                    {
                        found = true;
                        Console.WriteLine($"Using existing SAM file for date: {myDate:yyyy-MM-dd} (Julian: {myJul})");
                    }
                    else
                    {
                        string fileUrl = string.Format($@"https://api.sam.gov/data-services/v1/extracts?api_key={Globals.conSamApiKey}&fileName=SAM_Exclusions_Public_Extract_V2_{myJul}.ZIP");
                        Console.WriteLine($"Attempting to download SAM file for date: {myDate:yyyy-MM-dd} (Julian: {myJul})");

                        using (var downloadStream = await _httpClient.GetStreamAsync(fileUrl))
                        using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                        {
                            await downloadStream.CopyToAsync(fileStream);
                            found = true;
                            Console.WriteLine("SAM file downloaded successfully");
                        }
                    }

                    // Process the file if download was successful
                    if (found)
                    {
                        var totalTimer = System.Diagnostics.Stopwatch.StartNew();

                        //All good - unzip file
                        var unzipTimer = System.Diagnostics.Stopwatch.StartNew();
                        var GoodSAM = UnZipSAMtoList(myJul, filePath);
                        unzipTimer.Stop();
                        Console.WriteLine($"Unzip and parse CSV: {unzipTimer.ElapsedMilliseconds}ms ({GoodSAM.Count} SAM records)");

                        //Pre-filter and normalize SAM data in memory to reduce database load
                        var samLookup = GoodSAM
                            .GroupBy(p => new { First = p.First?.Trim().ToUpper(), Last = p.Last?.Trim().ToUpper() })
                            .Select(g => g.Key)
                            .Where(k => !string.IsNullOrEmpty(k.First) && !string.IsNullOrEmpty(k.Last))
                            .ToHashSet();
                        Console.WriteLine($"SAM unique names: {samLookup.Count}");

                        //Fetch eligible contacts from database with optimized query
                        var dbQueryTimer = System.Diagnostics.Stopwatch.StartNew();
                        var eligibleContacts = context.Contacts
                            .Where(c => (c.TypeContactIdfk == 27 || c.TypeContactIdfk == 36)
                                     && c.RegistrationStatus == "Approved"
                                     && c.Archived == null
                                     && c.Ssn != null
                                     && c.Ssn.Length == 11)
                            .Select(c => new { c.ContactIdpk, c.NameFirst, c.NameLast, c.Ssn })
                            .ToList()
                            .Where(c => c.Ssn != null && c.Ssn.Substring(0, 3) != "000" && c.Ssn.Substring(0, 3) != "666")
                            .ToList();
                        dbQueryTimer.Stop();
                        Console.WriteLine($"Database query for eligible contacts: {dbQueryTimer.ElapsedMilliseconds}ms ({eligibleContacts.Count} contacts)");

                        //Join in memory - much faster than database join
                        var joinTimer = System.Diagnostics.Stopwatch.StartNew();
                        var matchedContactIds = eligibleContacts
                            .Where(c => samLookup.Contains(new { First = c.NameFirst?.Trim().ToUpper(), Last = c.NameLast?.Trim().ToUpper() }))
                            .Select(c => c.ContactIdpk)
                            .ToList();
                        joinTimer.Stop();
                        Console.WriteLine($"In-memory join: {joinTimer.ElapsedMilliseconds}ms ({matchedContactIds.Count} matches)");

                        //Batch update ExclusionHits - fetch all SAM hits for today, filter in memory
                        var updateTimer = System.Diagnostics.Stopwatch.StartNew();
                        var matchedContactIdSet = new HashSet<int>(matchedContactIds);
                        var hitsToUpdate = context.ExclusionHits
                            .Where(e => e.DateRun == myDateOnly && e.TableHit == "SAM")
                            .ToList()
                            .Where(e => matchedContactIdSet.Contains(e.ContactIdfk))
                            .ToList();

                        foreach (var hit in hitsToUpdate)
                        {
                            hit.NameHit = "SAM";
                            hit.TableHit = "SAM";
                            hit.LikelyMatch = "Fail";
                        }
                        updateTimer.Stop();
                        Console.WriteLine($"Update exclusion hits: {updateTimer.ElapsedMilliseconds}ms ({hitsToUpdate.Count} records)");

                        // Save changes to the database
                        var saveTimer = System.Diagnostics.Stopwatch.StartNew();
                        context.SaveChanges();
                        saveTimer.Stop();
                        Console.WriteLine($"Save changes: {saveTimer.ElapsedMilliseconds}ms");

                        totalTimer.Stop();
                        Console.WriteLine($"TOTAL PROCESSING TIME: {totalTimer.ElapsedMilliseconds}ms ({totalTimer.Elapsed.TotalSeconds:F2} seconds)");

                        await SendEmail(Globals.conReportToEmail, "Success SAM", "SAM Exclusion Records Created", "<strong>SAM Exclusion Records Created</strong>");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"HTTP error downloading SAM file (day offset: {intBackDay}): {e.Message}");
                    --intBackDay;  // Try previous day
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error processing SAM file (day offset: {intBackDay}): {e.Message}");
                    await SendEmail(Globals.conReportToEmail, "BuildSAM() Error", $"Error: {e.Message}", $"<strong>Error:{e.Message}</strong>");
                    break;  // Stop on non-HTTP errors
                }
            }
        }

        public static List<Person> UnZipSAMtoList(string JulDate, string localFileName)
        {
            //Unzip the CSV file to temp path
            ZipFile.ExtractToDirectory(Path.Combine(localFileName, "Download.zip"), filePath, true);
            //Path to the unzipped CSV file
            string csvFilePath = Path.Combine(localFileName, $"SAM_Exclusions_Public_Extract_V2_{JulDate}.CSV");

            //Read all rows into the local path into lis GoodOIG.  Exclude those with bad birthdates and empty.
            var GoodSAM = File.ReadAllLines(csvFilePath)
                 .Skip(1) // skip header
                 .Select(line => line.Split(','))
                .Where(fields => fields[0] == "\"Individual\"" && fields[3] != "\"\"") // filter
                 .Select(fields => new Person
                 {
                     First = fields[3].Trim('"'),
                     Last = fields[5].Trim('"'),
                     BirthDate = DateOnly.FromDateTime(DateTime.Now)
                 })
             .ToList();
            //Return the list of Persons from SAM
            return GoodSAM;
        }

        //My goal is to store changing data like API KEYS and contact info
        internal static string GetSQLapiData(string myKey)
        {
            var context = new SAMOIGdat();
            var mylookedupRecord = context.ApiKeys.First(x => x.Api == myKey);

            return mylookedupRecord.ApiKey1;
        }

        internal static async Task MakeExclusionRecords()
        {
            //Add student records to ExclusionHits for SAM - Set all to initial 'Pass'
            using var Newcontext = new SAMOIGdat();
            var SAM = Newcontext.Contacts
                .Where(c => (c.TypeContactIdfk == 27 || c.TypeContactIdfk == 36)
                         && c.RegistrationStatus == "Approved"
                         && c.Archived == null)
                .Select(c => new ExclusionHit
                {
                    DateRun = DateOnly.FromDateTime(DateTime.Now),
                    ContactIdfk = c.ContactIdpk,
                    NameInput = (c.NameLast ?? "").ToUpper() + "," + (c.NameFirst ?? "").ToUpper(),
                    NameHit = (c.NameLast ?? "").ToUpper() + "," + (c.NameFirst ?? "").ToUpper(),
                    TableHit = "SAM",
                    City = c.City ?? "",
                    State = c.State ?? "",
                    Message = "Search SAM + ",
                    LikelyMatch = "Pass"
                })
                .Distinct() // Optional: depends on your entity equality
                .ToList();

            Newcontext.ExclusionHits.AddRange(SAM);
            Newcontext.SaveChanges();

            //Add student records to ExclusionHits for OIG - Set all to initial 'Pass'
            using var NewerContext = new SAMOIGdat();
            var hits = NewerContext.Contacts
                .Where(c => (c.TypeContactIdfk == 27 || c.TypeContactIdfk == 36)
                         && c.RegistrationStatus == "Approved"
                         && c.Archived == null)
                .Select(c => new ExclusionHit
                {
                    DateRun = DateOnly.FromDateTime(DateTime.Now),
                    ContactIdfk = c.ContactIdpk,
                    NameInput = (c.NameLast ?? "").ToUpper() + "," + (c.NameFirst ?? "").ToUpper(),
                    NameHit = (c.NameLast ?? "").ToUpper() + "," + (c.NameFirst ?? "").ToUpper(),
                    TableHit = "OIG",
                    City = c.City ?? "",
                    State = c.State ?? "",
                    Message = "Search SAM + ",
                    LikelyMatch = "Pass"
                })
                .Distinct() // Optional: depends on your entity equality
                .ToList();

            NewerContext.ExclusionHits.AddRange(hits);
            NewerContext.SaveChanges();
            await SendEmail(Globals.conReportToEmail, "Success OIG", "OIG Exclusion Records Created", "<strong>OIG Exclusion Records Created</strong>");
        }

        internal static async Task SendEmail(string toEmail, string subject, string plainTextContent, string htmlContent)
        {
            try
            {
                Console.WriteLine($"Attempting to send email to {toEmail} with subject '{subject}'");
                Console.WriteLine($"Using SendGrid API Key: {Globals.conSendGrid.Substring(0, Math.Min(10, Globals.conSendGrid.Length))}...");

                EmailService emailService = new(Globals.conSendGrid);
                bool success = await emailService.SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);

                if (success)
                {
                    Console.WriteLine($"SUCCESS: Email sent to {toEmail} with subject '{subject}'");
                }
                else
                {
                    Console.WriteLine($"WARNING: Email failed to send to {toEmail} with subject '{subject}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to send email to {toEmail}. Subject: '{subject}'. Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

    }

    //Person class to hold First, Last, and BirthDate
    public class Person
    {
        public required string First { get; set; }
        public required string Last { get; set; }
        public required DateOnly BirthDate { get; set; }
    }
}
