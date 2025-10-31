using consoleSAMOIG;

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