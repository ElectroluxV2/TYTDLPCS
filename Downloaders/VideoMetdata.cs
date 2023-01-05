using System.Text.Json.Serialization;

namespace TYTDLPCS.Downloaders;

public record VideoMetdata(
    string Id,
    string Extractor,
    string Title,
    int Duration,
    (string Id, string Url)[] Thumbnails,
    VideoMetdata?[]? Entries,
    [property: JsonPropertyName("original_url")]
    string Url
);