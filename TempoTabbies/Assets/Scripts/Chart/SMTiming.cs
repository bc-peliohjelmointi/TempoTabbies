using System.Collections.Generic;
using UnityEngine;

public static class SMTiming
{
    public struct ParsedNote
    {
        public int lane;
        public float time;
        public char type;

        public bool isHoldStart;
        public bool isHoldEnd;
        public float holdEndTime; // only valid for hold starts
    }

    public static List<ParsedNote> GetNoteTimes(SMFile sm, SMChart chart)
    {
        List<ParsedNote> notes = new();
        if (chart?.Measures == null || chart.Measures.Count == 0)
        {
            Debug.LogWarning("Chart has no measures!");
            return notes;
        }

        // Do NOT apply sm.Offset here — music offset is handled in GameManager
        float bpm = 120f;
        if (sm?.Bpms != null && sm.Bpms.Count > 0)
        {
            foreach (var kv in sm.Bpms)
            {
                bpm = kv.Value;
                break;
            }
        }

        float secPerBeat = 60f / bpm;
        float currentBeat = 0f;

        // track active holds
        Dictionary<int, ParsedNote> activeHolds = new();

        foreach (var measure in chart.Measures)
        {
            int rows = measure.Count;
            for (int i = 0; i < rows; i++)
            {
                string row = measure[i];
                if (string.IsNullOrWhiteSpace(row)) continue;

                float rowBeat = currentBeat + (4f * i / rows);
                float time = rowBeat * secPerBeat; // offset removed!

                for (int lane = 0; lane < row.Length; lane++)
                {
                    char c = row[lane];
                    if (c == '0') continue;

                    if (c == '1') // tap
                    {
                        notes.Add(new ParsedNote { lane = lane, time = time, type = '1' });
                    }
                    else if (c == '2') // hold start
                    {
                        var n = new ParsedNote { lane = lane, time = time, type = '2', isHoldStart = true };
                        activeHolds[lane] = n;
                    }
                    else if (c == '3') // hold end
                    {
                        if (activeHolds.TryGetValue(lane, out ParsedNote start))
                        {
                            start.holdEndTime = time;
                            notes.Add(start); // add the hold start
                            activeHolds.Remove(lane);
                        }
                        notes.Add(new ParsedNote { lane = lane, time = time, type = '3', isHoldEnd = true });
                    }
                }
            }
            currentBeat += 4f;
        }

        Debug.Log($"Generated {notes.Count} notes (with holds)");
        return notes;
    }

    public static ParsedNote? FindHoldEnd(List<ParsedNote> notes, int startIndex)
    {
        var start = notes[startIndex];
        if (start.type != '2') return null; // not a hold start

        for (int i = startIndex + 1; i < notes.Count; i++)
        {
            var n = notes[i];
            if (n.lane == start.lane && n.type == '3')
                return n;
        }
        return null;
    }
}
