﻿using HighHttpRequestCountDemo.API.Domain;
using HighHttpRequestCountDemo.Services;
using System.Diagnostics;
using System.Net;

namespace HighHttpRequestCountDemo;

internal class Program
{
    const string targetBaseUrl = "http://localhost:7550";
    const string targetSecureBaseUrl = "https://localhost:7560";
    static List<IDemoStrategy> availableStrategies = null!;

    const int NUMBER_OF_REQUESTS_TO_SEND = 5_000;

    private static void Main(string[] args)
    {
        SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler()
        {
            MaxConnectionsPerServer = 16,
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2)
        };
        HttpClient httpClient = new HttpClient(socketsHttpHandler); // Long lived
        List<IDemoStrategy> strategiesToDemo = [];

        availableStrategies = [new SemaphoreSlimStategy(httpClient, targetSecureBaseUrl),
                               new TransformBlockStrategy(httpClient, targetSecureBaseUrl),
                               new ConcurrentQueueStrategy(httpClient, targetSecureBaseUrl),
                               new BlockingCollectionStrategy(httpClient, targetSecureBaseUrl)
                               ];

        StartWebApi();

        if (WaitForWebApiReady(httpClient))
        {
            WriteLine($"\nWeb API is running.  Swagger also available at {targetSecureBaseUrl}/swagger.", ConsoleColor.Green);

            while (true)
            {
                strategiesToDemo.AddRange( DisplayMenu() );

                if (strategiesToDemo.Count == 0)
                {
                    return;
                }

                PerformDemo(strategiesToDemo, NUMBER_OF_REQUESTS_TO_SEND);
                strategiesToDemo.Clear();
            }
        }
        else
        {
            WriteLine("Web API did not start.", ConsoleColor.Red);
        }
    }

    private static void StartWebApi()
    {
        WriteLine("Starting Web API...", ConsoleColor.Yellow);

        Task.Run(() => API.Program.Main([]));
    }

    private static bool WaitForWebApiReady(HttpClient client)
    {
        using HttpRequestMessage headMsg = new HttpRequestMessage(HttpMethod.Head, targetBaseUrl);
        int attemptCnt = 0;

        while (++attemptCnt <= 3)
        {
            try
            {
                using HttpResponseMessage responseMsg = client.Send(headMsg);
                if (responseMsg.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch
            {
                Thread.Sleep(500);
            }
        }

        return false;
    }

    private static List<IDemoStrategy> DisplayMenu()
    {
        List<IDemoStrategy> demoStrategies = [];

        Console.WriteLine();
        for (int i = 0; i < availableStrategies.Count; i++)
        {
            WriteLine($"\t\t{i + 1}) {availableStrategies[i].Name}");
        }
        WriteLine("\t\ta) All Strategies\n\t\tx) Exit");

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Select a strategy to demo:  ");

        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.X:
                break;
            case ConsoleKey.A:
                demoStrategies.AddRange(availableStrategies);
                break;
            case ConsoleKey n when (n >= ConsoleKey.D1 && n <= ConsoleKey.D9):
                if (IsAvailableNumberKey(n, out int index))
                {
                    demoStrategies.Add(availableStrategies[index]);
                }
                break;
            case ConsoleKey n when (n >= ConsoleKey.NumPad1 && n <= ConsoleKey.NumPad9):
                if (IsAvailableNumberKey(n, out index))
                {
                    demoStrategies.Add(availableStrategies[index]);
                }
                break;
            default:
                break;
        }
        Console.WriteLine();

        return demoStrategies;

        static bool IsAvailableNumberKey(ConsoleKey key, out int strategyIndex)
        {
            strategyIndex = -1;

            if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9)
            {
                strategyIndex = key - ConsoleKey.D0 - 1;
            }
            else if (key >= ConsoleKey.NumPad0 && key <= ConsoleKey.NumPad9)
            {
                strategyIndex = key - ConsoleKey.NumPad0 - 1;
            }

            return strategyIndex >= 0 && strategyIndex < availableStrategies.Count;
        }
    }

    private static void PerformDemo(List<IDemoStrategy> strategiesToDemo, int numberOfRequests = 1_000)
    {
        WriteLine($"Performing {strategiesToDemo.Count} Demo(s) using {numberOfRequests:N0} requests...", ConsoleColor.Yellow);
        Stopwatch stopwatch = new Stopwatch();

        try
        {
            strategiesToDemo.ForEach(strategy =>
            {
                WriteLine($"\nExecuting {strategy.Name}:\n{strategy.Description}", ConsoleColor.DarkYellow);

                stopwatch.Restart();
                IReadOnlyCollection<User> result = strategy.Execute(numberOfRequests);
                stopwatch.Stop();

                int failedCount = result.Where(u => u.Year == -1).Count();

                WriteLine($"\n{result.Count:N0} Responses received, {failedCount} Failed, in {stopwatch.Elapsed:m\\:ss\\.ff}.", ConsoleColor.White);
            });
        }
        catch (Exception ex)
        {
            WriteLine($"Error: {ex}", ConsoleColor.Red);
        }
    }

    public static void WriteLine(string msg, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(msg);
        Console.ForegroundColor = ConsoleColor.Gray; // Is default.
    }
}