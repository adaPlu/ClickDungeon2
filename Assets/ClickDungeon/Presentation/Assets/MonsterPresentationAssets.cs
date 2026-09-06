using UnityEngine;

namespace ClickDungeon.Presentation.Assets
{
    /// <summary>
    /// Resolves production monster presentation variants while retaining the legacy/core sprite as
    /// a safe fallback. Monster content IDs remain authoritative; this class only maps presentation.
    /// </summary>
    public static class MonsterPresentationAssets
    {
        public static Sprite Core(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            assets?.SpriteFor(BaseId(monsterId, boss));

        public static Sprite Master(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "master", boss);

        public static Sprite Gameplay(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "gameplay", boss);

        public static Sprite Portrait(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "portrait", boss);

        public static Sprite Roster(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "roster", boss);

        public static Sprite Spawn(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "spawn", boss);

        public static Sprite Idle(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "idle", boss);

        public static Sprite Attack(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "attack", boss);

        public static Sprite Hit(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "hit", boss);

        public static Sprite Victory(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "victory", boss);

        public static Sprite Defeat(PresentationAssetDatabase assets, string monsterId, bool boss = false) =>
            Variant(assets, monsterId, "defeat", boss);

        public static Sprite Special(PresentationAssetDatabase assets, string monsterId, string state, bool boss = false) =>
            Variant(assets, monsterId, state, boss);

        public static string BaseId(string monsterId, bool boss = false)
        {
            if (string.IsNullOrWhiteSpace(monsterId)) return string.Empty;
            return (boss ? "boss." : "monster.") + monsterId.Trim().ToLowerInvariant();
        }

        private static Sprite Variant(PresentationAssetDatabase assets, string monsterId, string variant, bool boss)
        {
            if (assets == null || string.IsNullOrWhiteSpace(variant)) return null;
            string baseId = BaseId(monsterId, boss);
            if (string.IsNullOrEmpty(baseId)) return null;
            return assets.SpriteFor(baseId + "." + variant.Trim().ToLowerInvariant()) ?? assets.SpriteFor(baseId);
        }
    }
}
