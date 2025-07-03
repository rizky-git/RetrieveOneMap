using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ClosedXML.Excel; // <-- Add this at the top
using Serilog;

class OneMapTokenResponse
{
    public string access_token { get; set; }
    public string expiry_timestamp { get; set; }
}

class OneMapSearchResult
{
    public int found { get; set; }
    public int totalNumPages { get; set; }
    public int pageNum { get; set; }
    public List<OneMapSearchItem> results { get; set; }
}

class OneMapSearchItem
{
    public string SEARCHVAL { get; set; }
    public string BLK_NO { get; set; }
    public string ROAD_NAME { get; set; }
    public string BUILDING { get; set; }
    public string ADDRESS { get; set; }
    public string POSTAL { get; set; }
    public string X { get; set; }
    public string Y { get; set; }
    public string LATITUDE { get; set; }
    public string LONGITUDE { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        Directory.CreateDirectory("logs");
        var exportFolder = Path.Combine(AppContext.BaseDirectory, "exports");
        Directory.CreateDirectory(exportFolder);
        try
        {
            Console.WriteLine($"START : {DateTime.Now}");
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Information() // Set the minimum level here
                        .WriteTo.Console()
                        .WriteTo.File(
                            "logs/log-.txt",
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                        )
                        .CreateLogger();

            Log.Information("Application Starting at {Time}", DateTime.Now);

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var email = config["OneMap:Email"];
            var password = config["OneMap:Password"];
            var accessToken = config["OneMap:AccessToken"];
            string token;

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var tokenResponse = await GetAccessTokenAsync(email, password);
                token = tokenResponse.access_token;
            }
            else if (!string.IsNullOrEmpty(accessToken))
            {
                token = accessToken;
            }
            else
            {
                throw new Exception("Please check email, password, access token at appsettings.json");
            }

            //Console.WriteLine("Please input the search value:");
            //string searchInput = Console.ReadLine();

            //var results = await GetAllResults(searchInput, token);

            var allResults = new List<OneMapSearchItem>();

            int startPostalCode = int.TryParse(config["OneMap:StartPostalCode"], out var s) ? s : 0;
            int endPostalCode = int.TryParse(config["OneMap:EndPostalCode"], out var e) ? e : 999999;

            Log.Information($"Search Postal Code: {startPostalCode} - {endPostalCode}");

            //for (int i = startPostalCode; i <= endPostalCode; i++)
            //{
            //    string searchVal = i.ToString("D6");
            //    //Console.WriteLine($"Searching: {searchVal}");

            //    try
            //    {
            //        var results = await GetAllResults(searchVal, token);

            //        if (results.Count > 0)
            //        {
            //            Console.WriteLine($"Found: {allResults.Count} for {searchVal}");
            //            allResults.AddRange(results);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Failed to search {searchVal}: {ex.Message}");
            //    }
            //}

            var locker = new object();

            await Parallel.ForEachAsync(GeneratePostalCodes(startPostalCode, endPostalCode), new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (postal, ct) =>
            {
                try
                {
                    var res = await GetAllResults(postal, token);
                    if (res.Count > 0)
                    {
                        lock (locker)
                        {
                            allResults.AddRange(res);
                            Console.WriteLine($"✅ {postal}: {res.Count} record(s)");
                            Log.Information($"✅ {postal}: {res.Count} record(s)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ {postal}: {ex.Message}");
                    Log.Error(ex, "An error occurred");
                }
            });


            Console.WriteLine("Fetched address from OneMap:");
            //Console.WriteLine(JsonSerializer.Serialize(new
            //{
            //    TotalFound = allResults.Count,
            //    Results = allResults
            //}, new JsonSerializerOptions { WriteIndented = true }));

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var csvFilePath = Path.Combine(exportFolder, $"postal_data_{timestamp}.csv");
            var excelFilePath = Path.Combine(exportFolder, $"postal_data_{timestamp}.xlsx");

            SaveToCsv(allResults, csvFilePath);
            SaveToExcel(allResults, excelFilePath);

            Console.WriteLine($"END : {DateTime.Now}");
            Log.Information("Application finished successfully at {Time}", DateTime.Now);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
            Log.Error(ex, "An error occurred");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static IEnumerable<string> GeneratePostalCodes(int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            yield return i.ToString("D6");
        }
    }

    public static async Task<OneMapTokenResponse> GetAccessTokenAsync(string email, string password)
    {
        using var httpClient = new HttpClient();
        var url = "https://www.onemap.gov.sg/api/auth/post/getToken";

        var payload = new { email, password };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OneMapTokenResponse>(responseString);
    }

    public static async Task<OneMapSearchResult> SearchAddressAsync(string searchVal, string token, int pageNum = 1)
    {
        using var httpClient = new HttpClient();

        var url = $"https://www.onemap.gov.sg/api/common/elastic/search?searchVal={searchVal}&returnGeom=Y&getAddrDetails=Y&pageNum={pageNum}";
        httpClient.DefaultRequestHeaders.Clear();
        //httpClient.DefaultRequestHeaders.Add("Authorization", token);     //this is for add token on header

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OneMapSearchResult>(json);
    }

    public static async Task<List<OneMapSearchItem>> GetAllResults(string searchVal, string token)
    {
        var allResults = new List<OneMapSearchItem>();
        int page = 1;
        OneMapSearchResult result;

        do
        {
            result = await SearchAddressAsync(searchVal, token, page);
            if (result?.results != null)
                allResults.AddRange(result.results);

            page++;
        } while (result != null && page <= result.totalNumPages);

        return allResults;
    }

    public static void SaveToCsv(List<OneMapSearchItem> items, string filePath)
    {
        using var writer = new StreamWriter(filePath);
        writer.WriteLine("POSTALCODE,BUILDING,ADDRESS,ROAD_NAME,BLK_NO,LATITUDE,LONGITUDE,X,Y");

        foreach (var item in items)
        {
            writer.WriteLine($"{item.POSTAL},\"{item.ADDRESS}\",\"{item.BUILDING}\",\"{item.ROAD_NAME}\",{item.BLK_NO},{item.LATITUDE},{item.LONGITUDE},{item.X},{item.Y}");
        }

        Console.WriteLine($"Saved results to {filePath}");
    }

    public static void SaveToExcel(List<OneMapSearchItem> items, string filePath)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("PostalData");

        // Set headers
        worksheet.Cell(1, 1).Value = "POSTALCODE";
        worksheet.Cell(1, 2).Value = "BUILDING";
        worksheet.Cell(1, 3).Value = "ADDRESS";
        worksheet.Cell(1, 4).Value = "ROAD_NAME";
        worksheet.Cell(1, 5).Value = "BLK_NO";
        worksheet.Cell(1, 6).Value = "LATITUDE";
        worksheet.Cell(1, 7).Value = "LONGITUDE";
        worksheet.Cell(1, 8).Value = "X";
        worksheet.Cell(1, 9).Value = "Y";

        // Format postal code column as text
        worksheet.Column(1).Style.NumberFormat.Format = "@";

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            int row = i + 2;

            // Prefix with apostrophe ensures Excel keeps leading zeros
            worksheet.Cell(row, 1).Value = "'" + item.POSTAL; // postal code
            worksheet.Cell(row, 2).Value = item.BUILDING;
            worksheet.Cell(row, 3).Value = item.ADDRESS;
            worksheet.Cell(row, 4).Value = item.ROAD_NAME;
            worksheet.Cell(row, 5).Value = item.BLK_NO;
            worksheet.Cell(row, 6).Value = item.LATITUDE;
            worksheet.Cell(row, 7).Value = item.LONGITUDE;
            worksheet.Cell(row, 8).Value = item.X;
            worksheet.Cell(row, 9).Value = item.Y;
        }

        workbook.SaveAs(filePath);
        Console.WriteLine($"Saved Excel file to {filePath}");
    }

    public static void SaveUniquePostalCodeToCsv(List<OneMapSearchItem> items, string filePath)
    {
        using var writer = new StreamWriter(filePath);
        writer.WriteLine("POSTALCODE,BUILDING,ADDRESS,ROAD_NAME,BLK_NO,LATITUDE,LONGITUDE,X,Y");

        var seenPostals = new HashSet<string>();

        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.POSTAL) && seenPostals.Add(item.POSTAL))
            {
                writer.WriteLine($"{item.POSTAL},\"{item.ADDRESS}\",\"{item.BUILDING}\",\"{item.ROAD_NAME}\",{item.BLK_NO},{item.LATITUDE},{item.LONGITUDE},{item.X},{item.Y}");
            }
        }

        Console.WriteLine($"Saved unique postal results to {filePath}");
    }
}
