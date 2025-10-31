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
    Console.WriteLine($"FATAL ERROR: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");

    try
    {
        await GetData.SendEmail(
            Globals.conReportToEmail,
            "Program Fatal Error",
            $"Fatal Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
            $"<strong>Fatal Error:</strong> {ex.Message}<br/><br/><strong>Stack Trace:</strong><br/><pre>{ex.StackTrace}</pre>"
        );
    }
    catch
    {
        Console.WriteLine("ERROR: Failed to send error notification email");
    }

    Environment.Exit(1);
}