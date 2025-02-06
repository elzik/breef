using Elzik.Breef.Api.Presentation;
using System.Text.Json.Serialization;

namespace Elzik.Breef.Api;

[JsonSerializable(typeof(SourcePageRequest))]
[JsonSerializable(typeof(PublishedBreefResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
