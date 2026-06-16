using System.Text.Json.Serialization;

namespace IntegratedMeetingStudio.Models;

public class TimelineItem
{
    [JsonPropertyName("speaker")]
    public string Speaker { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } // Optional if available
}

public class SpeakerStat
{
    [JsonPropertyName("speaker")]
    public string Speaker { get; set; }

    [JsonPropertyName("speechCount")]
    public int SpeechCount { get; set; }

    [JsonPropertyName("speechRatio")]
    public double SpeechRatio { get; set; } // Percentage or 0-1

    [JsonPropertyName("aiAnalysis")]
    public string AiAnalysis { get; set; } // AI analysis of speaker's tone, key points, etc.
}

public class NextAgendaItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}
