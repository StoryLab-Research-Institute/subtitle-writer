using System;
using UnityEngine;

namespace StoryLabResearch.SubtitleWriter
{
    [Serializable]
    public struct SrtTimecode
    {
        public int Hours;
        public int Minutes;
        public float Seconds;

        public float ToSeconds() => Hours * 3600f + Minutes * 60f + Seconds;

        public static SrtTimecode FromSeconds(float totalSeconds)
        {
            int h = (int)(totalSeconds / 3600f);
            int m = (int)((totalSeconds % 3600f) / 60f);
            float s = totalSeconds % 60f;
            return new SrtTimecode { Hours = h, Minutes = m, Seconds = s };
        }
    }
}
