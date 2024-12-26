using HighHttpRequestCountDemo.API.Domain;

namespace HighHttpRequestCountDemo.Services;

internal interface IDemoStrategy
{
    string Name { get; }
    string Description { get; }
    IReadOnlyList<User> Execute(int numberOfRequests);
}
