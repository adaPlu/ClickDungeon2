#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ClickDungeon.Presentation.Assets;

namespace ClickDungeon.EditorTools
{
    /// <summary>
    /// Verifies that approved production art has been physically imported and can be mapped into the
    /// presentation database. Kept opt-in until the binary production pack lands in Art/Runtime.
    /// </summary>
    public static class ProductionArtValidator
    {
        private const string RuntimeArtRoot = "Assets/ClickDungeon/Art/Runtime";

        [MenuItem("ClickDungeon/Validate/Production Art Report")]
        public static void ReportMenu()
        {
            var report = BuildReport();
            Debug.Log(report.ToMultilineString());
        }

        [MenuItem("ClickDungeon/Validate/Production Art Strict")]
        public static void ValidateMenu()
        {
            ValidateOrThrow();
            Debug.Log("ClickDungeon production art validation passed.");
        }

        public static void ValidateOrThrow()
        {
            var report = BuildReport();
            if (report.MissingFiles.Count == 0 && report.UnmappedFiles.Count == 0 && report.NonSpriteFiles.Count == 0)
                return;

            throw new InvalidDataException(report.ToMultilineString());
        }

        public static ProductionArtReport BuildReport()
        {
            var missing = new List<string>();
            var unmapped = new List<string>();
            var nonSprite = new List<string>();
            var mappedIds = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (string fileName in ProductionArtManifest.RequiredRuntimePngNames())
            {
                string assetPath = RuntimeArtRoot + "/" + fileName;
                if (!File.Exists(assetPath))
                {
                    missing.Add(fileName);
                    continue;
                }

                string stem = Path.GetFileNameWithoutExtension(fileName);
                string presentationId = PresentationAssetId.FromRuntimeArtFile(stem);
                if (string.IsNullOrWhiteSpace(presentationId))
                {
                    unmapped.Add(fileName);
                    continue;
                }

                mappedIds[fileName] = presentationId;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite == null) nonSprite.Add(fileName);
            }

            return new ProductionArtReport(missing, unmapped, nonSprite, mappedIds);
        }

        public sealed class ProductionArtReport
        {
            public ProductionArtReport(
                IReadOnlyList<string> missingFiles,
                IReadOnlyList<string> unmappedFiles,
                IReadOnlyList<string> nonSpriteFiles,
                IReadOnlyDictionary<string, string> mappedIds)
            {
                MissingFiles = missingFiles;
                UnmappedFiles = unmappedFiles;
                NonSpriteFiles = nonSpriteFiles;
                MappedIds = mappedIds;
            }

            public IReadOnlyList<string> MissingFiles { get; }
            public IReadOnlyList<string> UnmappedFiles { get; }
            public IReadOnlyList<string> NonSpriteFiles { get; }
            public IReadOnlyDictionary<string, string> MappedIds { get; }
            public int RequiredCount => ProductionArtManifest.RequiredRuntimePngNames().Count();
            public int PresentCount => RequiredCount - MissingFiles.Count;
            public bool IsComplete => MissingFiles.Count == 0 && UnmappedFiles.Count == 0 && NonSpriteFiles.Count == 0;

            public string ToMultilineString()
            {
                var lines = new List<string>
                {
                    "ClickDungeon production art report",
                    $"Required: {RequiredCount}",
                    $"Present: {PresentCount}",
                    $"Missing: {MissingFiles.Count}",
                    $"Unmapped: {UnmappedFiles.Count}",
                    $"Not imported as Sprite: {NonSpriteFiles.Count}"
                };

                Append(lines, "Missing production PNGs", MissingFiles);
                Append(lines, "Unmapped production PNGs", UnmappedFiles);
                Append(lines, "PNG files not imported as Sprite", NonSpriteFiles);
                return string.Join("\n", lines);
            }

            private static void Append(List<string> lines, string title, IReadOnlyList<string> values)
            {
                if (values == null || values.Count == 0) return;
                lines.Add(title + ":");
                foreach (string value in values) lines.Add(" - " + value);
            }
        }
    }
}
#endif
