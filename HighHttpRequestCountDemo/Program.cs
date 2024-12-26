using HighHttpRequestCountDemo.API.Domain;
using HighHttpRequestCountDemo.Services;
using System.Diagnostics;
using System.Net;

namespace HighHttpRequestCountDemo;

internal class Program
{
    const string targetBaseUrl = "http://localhost:7550";
    const string targetSecureBaseUrl = "https://localhost:7560";
    static SocketsHttpHandler socketsHttpHandler = null!;
    static HttpClient httpClient = null!; // Long lived

    private static void Main(string[] args)
    {
        socketsHttpHandler = new SocketsHttpHandler() { MaxConnectionsPerServer = 16, PooledConnectionIdleTimeout = TimeSpan.FromMinutes(30) };
        httpClient = new HttpClient(socketsHttpHandler);

        StartWebApi();

        if (WaitForWebApiReady())
        {
            WriteLine($"\nWeb API is running.  Swagger also available at {targetSecureBaseUrl}/swagger.", ConsoleColor.Green);

            PerformDemo();
        }
        else
        {
            WriteLine("Web API did not start.", ConsoleColor.Red);
        }

        PressKeyToExit();
    }

    private static void StartWebApi()
    {
        WriteLine("Starting Web API...", ConsoleColor.Yellow);

        Task.Run(() => API.Program.Main([]));
    }

    private static bool WaitForWebApiReady()
    {
        using HttpRequestMessage headMsg = new HttpRequestMessage(HttpMethod.Head, targetBaseUrl);
        int attemptCnt = 0;

        while (++attemptCnt <= 3)
        {
            try
            {
                using HttpResponseMessage responseMsg = httpClient.Send(headMsg);
                if (responseMsg.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch
            {
                Thread.Sleep(250);
            }
        }

        return false;
    }

    private static void PerformDemo(int howManyUsers = 10_000)
    {
        List<IDemoStrategy> strategies = [
            new SemaphoreSlimStategy(httpClient, targetSecureBaseUrl),
            new TransformBlockStrategy(httpClient, targetSecureBaseUrl)];

        WriteLine($"Performing {strategies.Count} Demos using {howManyUsers:N0} requests for each...", ConsoleColor.Yellow);
        Stopwatch stopwatch = new Stopwatch();

        try
        {
            strategies.ForEach(strategy =>
            {
                WriteLine($"\nExecuting {strategy.Name}:\n{strategy.Description}");

                stopwatch.Restart();
                IReadOnlyCollection<User> result = strategy.Execute(howManyUsers);
                stopwatch.Stop();

                int failedCount = result.Where(u => u.Year == -1).Count();

                WriteLine($"{result.Count:N0} Responses received.  Failed: {failedCount}, in {stopwatch.Elapsed:m\\:ss\\.ff}.", ConsoleColor.White);
            });
        }
        catch (Exception ex)
        {
            WriteLine($"Error:  {ex.Message}", ConsoleColor.Red);
        }
    }

    private static void PressKeyToExit()
    {
        WriteLine("\nPress any key to exit.");
        Console.ReadKey();
        Console.WriteLine("\nExiting.");
    }

    private static void WriteLine(string msg, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(msg);
    }
}