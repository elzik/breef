using System.Text.Json.Serialization;

namespace Elzik.Breef.Api;

[JsonSerializable(typeof(Breef))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
