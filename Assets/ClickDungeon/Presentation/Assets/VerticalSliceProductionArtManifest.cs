using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ClickDungeon.Presentation.Assets
{
    public enum VerticalSliceProductionArtDisposition
    {
        Keep,
        Repair,
        Create
    }

    public sealed class VerticalSliceProductionArtManifestEntry
    {
        public string AssetId { get; internal set; }
        public string Path { get; internal set; }
        public string Category { get; internal set; }
        public string Role { get; internal set; }
        public string ReferenceKey { get; internal set; }
        public VerticalSliceProductionArtDisposition Disposition { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public double PivotX { get; internal set; }
        public double PivotY { get; internal set; }
        public string SequenceId { get; internal set; }
        public string ExpectedGuid { get; internal set; }
        public string BindingId { get; internal set; }
        public string ValidationStatus { get; internal set; }
    }

    public sealed class VerticalSliceProductionArtManifest
    {
        private VerticalSliceProductionArtManifest(IReadOnlyList<VerticalSliceProductionArtManifestEntry> entries)
        {
            Entries = entries;
        }

        public IReadOnlyList<VerticalSliceProductionArtManifestEntry> Entries { get; }

        public static VerticalSliceProductionArtManifest Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("Production-art manifest JSON is empty.");

            try
            {
                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;
                if (!root.TryGetProperty("entries", out JsonElement entriesElement) ||
                    entriesElement.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException("Production-art manifest must contain an entries array.");

                var entries = new List<VerticalSliceProductionArtManifestEntry>();
                var assetIds = new HashSet<string>(StringComparer.Ordinal);

                foreach (JsonElement element in entriesElement.EnumerateArray())
                {
                    string assetId = RequiredString(element, "assetId");
                    if (!assetIds.Add(assetId))
                        throw new InvalidOperationException("Duplicate production-art assetId: " + assetId);

                    string dispositionText = RequiredString(element, "disposition");
                    VerticalSliceProductionArtDisposition disposition;
                    switch (dispositionText)
                    {
                        case "KEEP":
                            disposition = VerticalSliceProductionArtDisposition.Keep;
                            break;
                        case "REPAIR":
                            disposition = VerticalSliceProductionArtDisposition.Repair;
                            break;
                        case "CREATE":
                            disposition = VerticalSliceProductionArtDisposition.Create;
                            break;
                        default:
                            throw new InvalidOperationException(
                                "Unsupported production-art disposition '" + dispositionText + "' for " + assetId + ".");
                    }

                    int width = RequiredInt(element, "width");
                    int height = RequiredInt(element, "height");
                    if (width <= 0 || height <= 0)
                        throw new InvalidOperationException("Production-art dimensions must be positive for " + assetId + ".");

                    double pivotX = RequiredDouble(element, "pivotX");
                    double pivotY = RequiredDouble(element, "pivotY");
                    if (pivotX < 0.0 || pivotX > 1.0 || pivotY < 0.0 || pivotY > 1.0)
                        throw new InvalidOperationException("Production-art pivots must be within [0,1] for " + assetId + ".");

                    string expectedGuid = OptionalString(element, "expectedGuid");
                    if (disposition == VerticalSliceProductionArtDisposition.Keep &&
                        string.IsNullOrWhiteSpace(expectedGuid))
                        throw new InvalidOperationException("KEEP entry must preserve expectedGuid for " + assetId + ".");

                    entries.Add(new VerticalSliceProductionArtManifestEntry
                    {
                        AssetId = assetId,
                        Path = OptionalString(element, "path"),
                        Category = OptionalString(element, "category"),
                        Role = OptionalString(element, "role"),
                        ReferenceKey = OptionalString(element, "referenceKey"),
                        Disposition = disposition,
                        Width = width,
                        Height = height,
                        PivotX = pivotX,
                        PivotY = pivotY,
                        SequenceId = OptionalString(element, "sequenceId"),
                        ExpectedGuid = expectedGuid,
                        BindingId = OptionalString(element, "bindingId"),
                        ValidationStatus = OptionalString(element, "validationStatus")
                    });
                }

                return new VerticalSliceProductionArtManifest(entries);
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException("Production-art manifest JSON is invalid.", exception);
            }
        }

        private static string RequiredString(JsonElement element, string propertyName)
        {
            string value = OptionalString(element, propertyName);
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Production-art manifest entry is missing " + propertyName + ".");
            return value;
        }

        private static string OptionalString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value) ||
                value.ValueKind == JsonValueKind.Null)
                return string.Empty;
            if (value.ValueKind != JsonValueKind.String)
                throw new InvalidOperationException("Production-art property " + propertyName + " must be a string.");
            return value.GetString() ?? string.Empty;
        }

        private static int RequiredInt(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value) ||
                value.ValueKind != JsonValueKind.Number ||
                !value.TryGetInt32(out int result))
                throw new InvalidOperationException("Production-art property " + propertyName + " must be an integer.");
            return result;
        }

        private static double RequiredDouble(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value) ||
                value.ValueKind != JsonValueKind.Number ||
                !value.TryGetDouble(out double result))
                throw new InvalidOperationException("Production-art property " + propertyName + " must be numeric.");
            return result;
        }
    }
}
