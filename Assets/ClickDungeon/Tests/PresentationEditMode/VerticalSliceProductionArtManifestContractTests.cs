using System;
using NUnit.Framework;
using ClickDungeon.Presentation.Assets;

namespace ClickDungeon.Tests.PresentationEditMode
{
    public sealed class VerticalSliceProductionArtManifestContractTests
    {
        [Test]
        public void Parse_RejectsUnclassifiedDisposition()
        {
            const string json = "{\"entries\":[{\"assetId\":\"hero.clickington.idle\",\"disposition\":\"UNCLASSIFIED\"}]}";

            Assert.Throws<InvalidOperationException>(() => VerticalSliceProductionArtManifest.Parse(json));
        }

        [Test]
        public void Parse_RejectsDuplicateAssetIds()
        {
            const string json = "{\"entries\":[{\"assetId\":\"hero.clickington.idle\",\"disposition\":\"KEEP\",\"width\":512,\"height\":512,\"pivotX\":0.5,\"pivotY\":0.0,\"expectedGuid\":\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\"},{\"assetId\":\"hero.clickington.idle\",\"disposition\":\"CREATE\",\"width\":512,\"height\":512,\"pivotX\":0.5,\"pivotY\":0.0}]}";

            Assert.Throws<InvalidOperationException>(() => VerticalSliceProductionArtManifest.Parse(json));
        }

        [Test]
        public void Parse_RejectsInvalidDimensionsOrPivot()
        {
            const string zeroWidth = "{\"entries\":[{\"assetId\":\"hero.clickington.idle\",\"disposition\":\"CREATE\",\"width\":0,\"height\":512,\"pivotX\":0.5,\"pivotY\":0.0}]}";
            const string badPivot = "{\"entries\":[{\"assetId\":\"hero.clickington.idle\",\"disposition\":\"CREATE\",\"width\":512,\"height\":512,\"pivotX\":1.25,\"pivotY\":0.0}]}";

            Assert.Throws<InvalidOperationException>(() => VerticalSliceProductionArtManifest.Parse(zeroWidth));
            Assert.Throws<InvalidOperationException>(() => VerticalSliceProductionArtManifest.Parse(badPivot));
        }

        [Test]
        public void Parse_RejectsKeepWithoutExpectedGuid()
        {
            const string json = "{\"entries\":[{\"assetId\":\"hero.clickington.idle\",\"disposition\":\"KEEP\",\"width\":512,\"height\":512,\"pivotX\":0.5,\"pivotY\":0.0}]}";

            Assert.Throws<InvalidOperationException>(() => VerticalSliceProductionArtManifest.Parse(json));
        }
    }
}
