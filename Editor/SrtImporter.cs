using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace StoryLabResearch.SubtitleWriter.Editor
{
    [ScriptedImporter(1, "srt")]
    public class SrtImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string text = File.ReadAllText(ctx.assetPath);
            var asset = new TextAsset(text);
            ctx.AddObjectToAsset("text", asset);
            ctx.SetMainObject(asset);
        }
    }
}
