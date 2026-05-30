using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StoryLabResearch.SubtitleWriter
{
    public readonly struct SubtitleCue
    {
        public readonly float StartTime;
        public readonly float EndTime;
        public readonly string Text;

        public SubtitleCue(float startTime, float endTime, string text)
        {
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }
    }

    public static class SrtParser
    {
        // Matches "HH:MM:SS,mmm --> HH:MM:SS,mmm"
        private static readonly Regex TimecodePattern = new Regex(
            @"(\d{2}):(\d{2}):(\d{2}),(\d{3})\s*-->\s*(\d{2}):(\d{2}):(\d{2}),(\d{3})",
            RegexOptions.Compiled);

        public static List<SubtitleCue> Parse(string srtText)
        {
            var cues = new List<SubtitleCue>();
            if (string.IsNullOrEmpty(srtText))
                return cues;

            // Normalise line endings
            srtText = srtText.Replace("\r\n", "\n").Replace("\r", "\n");

            // Split into blocks on blank lines
            string[] blocks = srtText.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string block in blocks)
            {
                string[] lines = block.Trim().Split('\n');
                if (lines.Length < 2)
                    continue;

                // Find the timecode line (skip the index line)
                int timecodeLineIndex = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (TimecodePattern.IsMatch(lines[i]))
                    {
                        timecodeLineIndex = i;
                        break;
                    }
                }

                if (timecodeLineIndex < 0)
                    continue;

                Match m = TimecodePattern.Match(lines[timecodeLineIndex]);
                float start = ParseTime(m, 1);
                float end = ParseTime(m, 5);

                var textBuilder = new StringBuilder();
                for (int i = timecodeLineIndex + 1; i < lines.Length; i++)
                {
                    if (textBuilder.Length > 0)
                        textBuilder.Append('\n');
                    textBuilder.Append(lines[i].Trim());
                }

                string text = textBuilder.ToString();
                if (string.IsNullOrEmpty(text))
                    continue;

                cues.Add(new SubtitleCue(start, end, text));
            }

            return cues;
        }

        private static float ParseTime(Match m, int groupOffset)
        {
            float hours = float.Parse(m.Groups[groupOffset].Value);
            float minutes = float.Parse(m.Groups[groupOffset + 1].Value);
            float seconds = float.Parse(m.Groups[groupOffset + 2].Value);
            float ms = float.Parse(m.Groups[groupOffset + 3].Value);
            return hours * 3600f + minutes * 60f + seconds + ms / 1000f;
        }
    }
}
