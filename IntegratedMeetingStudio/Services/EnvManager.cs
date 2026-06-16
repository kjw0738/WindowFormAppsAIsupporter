using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IntegratedMeetingStudio.Services;

public static class EnvManager
{
    private static string GetEnvFilePath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
    }

    public static void Set(string key, string value)
    {
        var filePath = GetEnvFilePath();
        var lines = new List<string>();

        if (File.Exists(filePath))
        {
            lines = File.ReadAllLines(filePath).ToList();
        }

        bool found = false;
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith($"{key}=") || line.StartsWith($"{key} ="))
            {
                lines[i] = $"{key}={value}";
                found = true;
                break;
            }
        }

        if (!found)
        {
            lines.Add($"{key}={value}");
        }

        File.WriteAllLines(filePath, lines);
        Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
    }

    public static string Get(string key, string defaultValue = "")
    {
        var filePath = GetEnvFilePath();
        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith($"{key}=") || trimmed.StartsWith($"{key} ="))
                {
                    var parts = trimmed.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        return parts[1].Trim();
                    }
                }
            }
        }
        
        var envVar = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        if (!string.IsNullOrEmpty(envVar)) return envVar;

        return defaultValue;
    }
}
