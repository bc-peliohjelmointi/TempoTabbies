using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class SMChart
{
    public string Type;
    public string Description;
    public string Difficulty;
    public int Meter;
    public List<List<string>> Measures = new();
}

public class SMFile
{
    public string Title;
    public string Artist;
    public string MusicFile;    
    public float Offset;
    public Dictionary<float, float> Bpms = new();
    public List<SMChart> Charts = new();

    public string FilePath;  // full path to the .sm file
    public string DirectoryPath => Path.GetDirectoryName(FilePath);
}

public static class SMParser
{
    public static SMFile Parse(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"SM file not found: {path}");
            return null;
            
        }

        string data = File.ReadAllText(path);
        SMFile sm = new();
        sm.FilePath = path;

        sm.Title = GetTag(data, "TITLE");
        sm.Artist = GetTag(data, "ARTIST");
        sm.MusicFile = GetTag(data, "MUSIC");
        sm.Offset = float.Parse(GetTag(data, "OFFSET", "0"), System.Globalization.CultureInfo.InvariantCulture);
        sm.Bpms = ParseBpms(GetTag(data, "BPMS"));

        var noteBlocks = Regex.Matches(data, @"#NOTES:(.*?);", RegexOptions.Singleline);
        foreach (Match m in noteBlocks)
        {
            string[] lines = m.Groups[1].Value.Split('\n');
            List<string> trimmed = new();
            foreach (var line in lines)
            {
                string t = line.Trim();
                if (!string.IsNullOrEmpty(t))
                    trimmed.Add(t);
            }

            if (trimmed.Count < 6) continue;

            SMChart chart = new SMChart();
            chart.Type = trimmed[0].Trim(':');

            // Only read dance-solo charts
            if (!chart.Type.ToLower().Contains("dance-solo"))
                continue;

            chart.Description = trimmed[1].Trim(':');
            chart.Difficulty = trimmed[2].Trim(':');
            chart.Meter = int.Parse(trimmed[3].Trim(':'));

            string noteData = string.Join("\n", trimmed.GetRange(5, trimmed.Count - 5));
            string[] measures = noteData.Split(',');
            foreach (string measure in measures)
            {
                List<string> rows = new();
                foreach (string line in measure.Split('\n'))
                {
                    string l = line.Trim();
                    if (l.Length > 0 && Regex.IsMatch(l, @"^[0-9]+$"))
                        rows.Add(l);
                }
                if (rows.Count > 0)
                    chart.Measures.Add(rows);
            }

            sm.Charts.Add(chart);
        }

        return sm;
    }

    static string GetTag(string data, string tag, string def = "")
    {
        Match m = Regex.Match(data, $"#{tag}:([^;]*);");
        return m.Success ? m.Groups[1].Value.Trim() : def;
    }

    static Dictionary<float, float> ParseBpms(string bpmData)
    {
        Dictionary<float, float> map = new();
        if (string.IsNullOrEmpty(bpmData)) return map;
        string[] pairs = bpmData.Split(',');
        foreach (string p in pairs)
        {
            string[] kv = p.Split('=');
            if (kv.Length == 2)
            {
                if (float.TryParse(kv[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float beat) &&
                    float.TryParse(kv[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float bpm))
                {
                    map[beat] = bpm;
                }
            }
        }
        return map;
    }
}
