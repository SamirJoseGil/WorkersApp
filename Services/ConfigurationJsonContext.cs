using System.Text.Json.Serialization;
using WorkersApp.Models;

namespace WorkersApp.Services
{
    [JsonSerializable(typeof(Configuration))]
    public partial class ConfigurationJsonContext : JsonSerializerContext
    {
    }
}
