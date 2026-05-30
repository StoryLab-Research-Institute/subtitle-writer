using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryLab.SubtitleWriter
{
    [Serializable]
    public class SubtitleTrack
    {
        public string LanguageCode;
        public TextAsset SrtAsset;
    }

    public enum SubtitleStartMode
    {
        OnGameObjectStart,
        Manual
    }

    public class SubtitleReader : MonoBehaviour
    {
        [Header("Tracks")]
        [SerializeField] private List<SubtitleTrack> _tracks = new List<SubtitleTrack>();
        [SerializeField] private string _languageCode = "EN";

        [Header("Playback")]
        [SerializeField] private SubtitleStartMode _startMode = SubtitleStartMode.OnGameObjectStart;
        [SerializeField] private float _startFrom = 0f;
        [SerializeField] private float _stopAt = 0f;

        public string CurrentSubtitle { get; private set; } = string.Empty;

        private Dictionary<string, List<SubtitleCue>> _cuesByLanguage;
        private List<SubtitleTrack> _lastTracksSnapshot;

        private bool _isPlaying;
        private float _elapsed;
        private float _playStartFrom;
        private float _playStopAt;

        // Binary search state: track last index to avoid scanning from zero every frame
        private int _lastCueIndex;

        private void Start()
        {
            if (_startMode == SubtitleStartMode.OnGameObjectStart)
                Play(_startFrom, _stopAt);
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            _elapsed += Time.deltaTime;

            if (_playStopAt > 0f && _elapsed >= _playStopAt)
            {
                CurrentSubtitle = string.Empty;
                _isPlaying = false;
                return;
            }

            CurrentSubtitle = GetCueAt(_elapsed);
        }

        // --- Public API ---

        public void Play() => BeginPlayback(_startFrom, _stopAt);
        public void Play(float startFrom) => BeginPlayback(startFrom, _stopAt);
        public void Play(float startFrom, float stopAt) => BeginPlayback(startFrom, stopAt);

        public void Stop()
        {
            _isPlaying = false;
            _elapsed = _playStartFrom;
            _lastCueIndex = 0;
            CurrentSubtitle = string.Empty;
        }

        public void Pause() => _isPlaying = false;

        public void Resume() => _isPlaying = true;

        public string LanguageCode
        {
            get => _languageCode;
            set
            {
                _languageCode = value;
                _lastCueIndex = 0;
                CurrentSubtitle = _isPlaying ? GetCueAt(_elapsed) : string.Empty;
            }
        }

        // --- Internal ---

        private void BeginPlayback(float startFrom, float stopAt)
        {
            _playStartFrom = startFrom;
            _playStopAt = stopAt;
            _elapsed = startFrom;
            _lastCueIndex = 0;
            _isPlaying = true;
            CurrentSubtitle = GetCueAt(_elapsed);
        }

        private string GetCueAt(float time)
        {
            RebuildIfDirty();

            if (!_cuesByLanguage.TryGetValue(_languageCode, out List<SubtitleCue> cues) || cues.Count == 0)
                return string.Empty;

            // Walk forward from last known index (common case: time moves forward)
            // Reset backwards if time has jumped back
            if (_lastCueIndex > 0 && time < cues[_lastCueIndex - 1].StartTime)
                _lastCueIndex = 0;

            for (int i = _lastCueIndex; i < cues.Count; i++)
            {
                if (time < cues[i].StartTime)
                    break;

                if (time <= cues[i].EndTime)
                {
                    _lastCueIndex = i;
                    return cues[i].Text;
                }

                // Past the end of this cue — advance hint
                _lastCueIndex = i + 1;
            }

            return string.Empty;
        }

        private void RebuildIfDirty()
        {
            if (_cuesByLanguage != null && !TrackListChanged())
                return;

            _cuesByLanguage = new Dictionary<string, List<SubtitleCue>>(StringComparer.OrdinalIgnoreCase);

            foreach (SubtitleTrack track in _tracks)
            {
                if (track == null || string.IsNullOrEmpty(track.LanguageCode) || track.SrtAsset == null)
                    continue;

                _cuesByLanguage[track.LanguageCode] = SrtParser.Parse(track.SrtAsset.text);
            }

            SnapshotTracks();
        }

        private bool TrackListChanged()
        {
            if (_lastTracksSnapshot == null || _lastTracksSnapshot.Count != _tracks.Count)
                return true;

            for (int i = 0; i < _tracks.Count; i++)
            {
                if (_lastTracksSnapshot[i] != _tracks[i] ||
                    _lastTracksSnapshot[i]?.SrtAsset != _tracks[i]?.SrtAsset ||
                    _lastTracksSnapshot[i]?.LanguageCode != _tracks[i]?.LanguageCode)
                    return true;
            }

            return false;
        }

        private void SnapshotTracks()
        {
            _lastTracksSnapshot = new List<SubtitleTrack>(_tracks.Count);
            foreach (SubtitleTrack track in _tracks)
                _lastTracksSnapshot.Add(track);
        }
    }
}
