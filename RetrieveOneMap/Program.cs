using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

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
        try
        {
            Console.WriteLine($"START : {DateTime.Now}");

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

            for (int i = 18900; i <= 999999; i++)
            {
                string searchVal = i.ToString("D6");
                //Console.WriteLine($"Searching: {searchVal}");

                try
                {
                    var results = await GetAllResults(searchVal, token);

                    if (results.Count > 0)
                    {
                        Console.WriteLine($"Found: {allResults.Count} for {searchVal}");
                        allResults.AddRange(results);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to search {searchVal}: {ex.Message}");
                }
            }

            Console.WriteLine("Fetched address from OneMap:");
            //Console.WriteLine(JsonSerializer.Serialize(new
            //{
            //    TotalFound = allResults.Count,
            //    Results = allResults
            //}, new JsonSerializerOptions { WriteIndented = true }));

            //SaveToCsv(allResults, $"postal_data_{searchInput}.csv");
            SaveToCsv(allResults, $"postal_data_all_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv");
            Console.WriteLine($"END : {DateTime.Now}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
            throw;
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
