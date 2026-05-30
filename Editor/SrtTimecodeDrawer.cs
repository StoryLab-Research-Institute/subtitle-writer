using StoryLabResearch.SubtitleWriter;
using UnityEditor;
using UnityEngine;

namespace StoryLabResearch.SubtitleWriter.Editor
{
    [CustomPropertyDrawer(typeof(SrtTimecode))]
    public class SrtTimecodeDrawer : PropertyDrawer
    {
        private const float LabelWidth = 18f;
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            var hoursRect   = SliceLeft(ref position, (position.width - LabelWidth * 3 - Spacing * 5) / 3f);
            var colonRect1  = SliceLeft(ref position, LabelWidth);
            var minutesRect = SliceLeft(ref position, (position.width - LabelWidth * 2 - Spacing * 3) / 2f);
            var colonRect2  = SliceLeft(ref position, LabelWidth);
            var secondsRect = position;

            var hours   = property.FindPropertyRelative("Hours");
            var minutes = property.FindPropertyRelative("Minutes");
            var seconds = property.FindPropertyRelative("Seconds");

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            hours.intValue   = Mathf.Clamp(EditorGUI.IntField(hoursRect, hours.intValue), 0, 99);
            EditorGUI.LabelField(colonRect1, ":");
            minutes.intValue = Mathf.Clamp(EditorGUI.IntField(minutesRect, minutes.intValue), 0, 59);
            EditorGUI.LabelField(colonRect2, ":");
            seconds.floatValue = Mathf.Clamp(EditorGUI.FloatField(secondsRect, seconds.floatValue), 0f, 59.999f);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        private static Rect SliceLeft(ref Rect rect, float width)
        {
            var slice = new Rect(rect.x, rect.y, width, rect.height);
            rect.xMin += width + Spacing;
            return slice;
        }
    }
}
