namespace MusicWebApp.Models;

public sealed record SearchFilters(
    string Sort = "relevance",
    string Period = "any",
    string Duration = "any",
    string Language = "any",
    bool EmbeddableOnly = true)
{
    public bool IsDefault => this == new SearchFilters();

    public static SearchFilters Normalize(string? sort, string? period, string? duration, string? language, bool? embeddable)
        => new(
            sort is "date" or "viewCount" ? sort : "relevance",
            period is "year" or "since2025" ? period : "any",
            duration is "under5" or "from5to20" or "over20" ? duration : "any",
            language is "vi" or "en" or "ja" or "zh-Hans" ? language : "any",
            embeddable ?? true);

    public bool MatchesDuration(MusicTrack track) => Duration switch
    {
        "under5" => track.DurationSeconds is > 0 and < 300,
        "from5to20" => track.DurationSeconds is >= 300 and <= 1200,
        "over20" => track.DurationSeconds is > 1200,
        _ => true
    };
}
