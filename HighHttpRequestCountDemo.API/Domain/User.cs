using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

// TODO: Note - With latest VS2022 17.12.3, Cannot find template for class library targeting any .NET Core, only .NET Framework.
// So, for just this simple demo, the User DTO object will be put here.
namespace HighHttpRequestCountDemo.API.Domain;

public class User
{
    /// <summary> The user's Id </summary>
    [Range(1, 1_000_000)]
    public int Id { get; set; }

    /// <summary> The year since the user has been a member. </summary>
    public short Year { get; set; }
}
