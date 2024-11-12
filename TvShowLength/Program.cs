/*
Author: Mariusz Trzeciak
Used resources:
https://learn.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client
https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient
https://www.newtonsoft.com/json/help/html/Introduction.htm
*/

using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace TvShowLength;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        // Simple check if command line argument is specified
        if (args.Length < 1 || args.Length > 2)
        {
            Console.Error.WriteLine("Incorrect number of arguments.");
            return 1;
        }

        // Use the first argument as the show titledsd 
        var showTitle = args[0];

        // Http client config
        HttpClient client = new();
        client.BaseAddress = new Uri("https://api.tvmaze.com/");
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

        // Attempt to fetch data from the API
        // Return codes: 0 - success, 10 - show not found, 1 - failure
        try
        {
            using (client)
            {
                // Make a GET request to search for shows with the specified title
                var response = await client.GetAsync($"search/shows?q={showTitle}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Could not get info for {showTitle}.");
                    return 1;
                }

                // Parse the JSON response into a JArray to easily manipulate JSON data
                var showInformation = JArray.Parse(await response.Content.ReadAsStringAsync());

                // Check if the search returned any results
                if (!showInformation.Any())
                {
                    Console.Error.WriteLine($"Could not get info for {showTitle}.");
                    return 10;
                }

                // Select the most recent show based on its premiere date
                var selectedShow = showInformation
                    .OrderByDescending(s => (DateTime?)s["show"]!["premiered"])
                    .First();
                var showId = selectedShow["show"]!["id"]!.ToString();

                // Make a GET request to retrieve episodes of the selected show
                var episodesResponse = await client.GetAsync($"shows/{showId}/episodes");

                if (!episodesResponse.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Could not get info for {showTitle}.");
                    return 1;
                }

                // Parse the JSON response into a JArray to easily manipulate JSON data
                var episodes = JArray.Parse(await episodesResponse.Content.ReadAsStringAsync());

                // Calculate the total length of the show by summing the runtime of each episode
                var totalRuntime = episodes
                    .Where(episode => episode["runtime"] != null)
                    .Sum(episode => (int)episode["runtime"]!);

                Console.WriteLine(totalRuntime);

                return 0;
            }
        }
        catch (Exception ex)
        {
            // Log any exceptions
            Console.Error.WriteLine($"Could not get info for {showTitle}. Error: {ex.Message}");
            return 1;
        }
    }

    public Task CalculateX598HashAsync()
    {
        throw new NotImplementedException();
    }
}