using HighHttpRequestCountDemo.API.Domain;

namespace HighHttpRequestCountDemo.API
{
    public static class AppExtensions
    {
        /// <summary>
        /// Add the HEAD and Get User minimal APIs needed for this demo app.
        /// </summary>
        /// <param name="app"></param>
        public static void AddMinimalApis(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.MapMethods("/", ["HEAD"], () => Results.Ok());

            const int FirstYearInBusiness = 1965;

            app.MapGet("/user/{Id}", (int Id) =>
            {
                return new User() { Id = Id, Year = (short)Random.Shared.Next(FirstYearInBusiness, DateTime.Now.Year) };
            })
            .AddEndpointFilter(async (invocationContext, next) =>
            {
                int id = invocationContext.GetArgument<int>(0);

                if (id < 1 || id > 1_000_000)
                {
                    return Results.Problem("Invalid user Id value.");
                }
                return await next(invocationContext);
            });
        }
    }
}
