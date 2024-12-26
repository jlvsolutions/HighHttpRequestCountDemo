using HighHttpRequestCountDemo.API.Domain;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace HighHttpRequestCountDemo.Services;

internal static class HttpExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Guarantees the returns of an <see cref="User"/> object regardless of errors.  If an error occurrs
    /// the returned User object will have the Year set to -1
    /// </summary>
    /// <returns>An User instance.  Year will be set to -1 if an error occurred.</returns>
    public static async Task<User> GetUser(this HttpClient client, string userUrl)
    {
        ArgumentNullException.ThrowIfNull(client);

        using HttpResponseMessage response = await client.GetAsync(userUrl);

        if (!response.IsSuccessStatusCode)
        {
            return ErrorUser(userUrl);
        }

        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<User>(content, JsonOptions) ?? ErrorUser(userUrl);
            
        // Used to indicate an error
        static User ErrorUser(string url)
        {
            return new User
            {
                Id = int.Parse(url[(url.LastIndexOf('/') + 1)..]), // Cheap, just for this demo
                Year = -1
            };
        }
    }
}
