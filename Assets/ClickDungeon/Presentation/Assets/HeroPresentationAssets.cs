using UnityEngine;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.Assets
{
    /// <summary>
    /// Resolves high-fidelity hero presentation variants while retaining the legacy/core sprite as
    /// a safe fallback. This lets art production advance without changing stable gameplay class IDs.
    /// </summary>
    public static class HeroPresentationAssets
    {
        public static Sprite Core(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            assets?.SpriteFor(BaseId(heroClass));

        public static Sprite Master(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, heroClass, "master");

        public static Sprite Gameplay(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, heroClass, "gameplay");

        public static Sprite Portrait(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, heroClass, "portrait");

        public static Sprite Roster(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, heroClass, "roster");

        public static Sprite Victory(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, heroClass, "victory");

        public static Sprite Defeat(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, heroClass, "defeat");

        public static string BaseId(HeroClassId heroClass) =>
            "hero." + heroClass.ToString().ToLowerInvariant();

        private static Sprite Variant(PresentationAssetDatabase assets, HeroClassId heroClass, string variant)
        {
            if (assets == null) return null;
            string baseId = BaseId(heroClass);
            return assets.SpriteFor(baseId + "." + variant) ?? assets.SpriteFor(baseId);
        }
    }
}
