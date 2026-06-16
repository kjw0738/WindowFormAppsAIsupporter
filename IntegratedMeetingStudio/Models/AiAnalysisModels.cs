using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IntegratedMeetingStudio.Models;

public class SwotAnalysisResult
{
    [JsonPropertyName("Strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("Weaknesses")]
    public List<string> Weaknesses { get; set; } = new();

    [JsonPropertyName("Opportunities")]
    public List<string> Opportunities { get; set; } = new();

    [JsonPropertyName("Threats")]
    public List<string> Threats { get; set; } = new();
}

public class RoleDistributionResult
{
    [JsonPropertyName("Participant")]
    public string Participant { get; set; } = string.Empty;

    [JsonPropertyName("Roles")]
    public List<string> Roles { get; set; } = new();
}
