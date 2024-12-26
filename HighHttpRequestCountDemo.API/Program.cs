namespace HighHttpRequestCountDemo.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions() 
        { 
            EnvironmentName = "Development", 
            ApplicationName = "HighHttpRequestCountDemo.Api"
        });

        // Configure the URLs
        builder.WebHost.UseUrls(["http://localhost:7550", "https://localhost:7560"]);

        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // NOTE:  If desired, builder.Services.AddRateLimiter(... middleware could be applied here.

        // Build the application.
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Add the Apis for this demo app.
        app.AddMinimalApis();

        app.Run();
    }
}