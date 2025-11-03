using consoleSAMOIG;

try
{
    //Initialize configuration
    Globals.Initialize();

    //First Build Pass SAM and OIG Exclusion Records
    //This will build Pass Exclusion Records in SAM and OIG - I commented out SAM
    //GetData.MakeExclusionRecords();

    await GetData.BuildOIG();
    Console.WriteLine("BuildOIG Processing Complete");
    await GetData.BuildSAM();
    Console.WriteLine("BuildSAM Processing Complete");
    //Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR: {ex.ToString()}");

    try
    {
        await GetData.SendEmail(
            Globals.conReportToEmail,
            "Program Fatal Error",
            $"Complete Exception Details:\n{ex.ToString()}",
            $"<strong>Complete Exception Details:</strong><br/><pre>{System.Net.WebUtility.HtmlEncode(ex.ToString())}</pre>"
        );
    }
    catch
    {
        Console.WriteLine("ERROR: Failed to send error notification email");
    }

    Environment.Exit(1);
}