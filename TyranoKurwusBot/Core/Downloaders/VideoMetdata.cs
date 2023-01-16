using System.Text.Json.Serialization;

namespace TyranoKurwusBot.Core.Downloaders;

public record VideoMetadata(
    string Id,
    string Extractor,
    string Title,
    int Duration,
    (string Id, string Url)[] Thumbnails,
    VideoMetadata?[]? Entries,
    [property: JsonPropertyName("original_url")]
    string Url
);