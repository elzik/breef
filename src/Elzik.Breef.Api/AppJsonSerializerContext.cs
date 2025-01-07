using Elzik.Breef.Api.Presentation;
using System.Text.Json.Serialization;

namespace Elzik.Breef.Api;

[JsonSerializable(typeof(SourcePageRequest))]
[JsonSerializable(typeof(BreefResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
