using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ClosedXML.Excel; // <-- Add this at the top
using Serilog;

namespace RetrieveOneMap
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            string accessToken = config["OneMap:AccessToken"];
            string email = config["OneMap:Email"];
            string password = config["OneMap:Password"];
            int startPostalCode = int.TryParse(config["OneMap:StartPostalCode"], out var s) ? s : 0;
            int endPostalCode = int.TryParse(config["OneMap:EndPostalCode"], out var e) ? e : 999999;

            await OneMapHelper.RunExtractionAsync(accessToken, email, password, startPostalCode, endPostalCode);
        }
    }
}