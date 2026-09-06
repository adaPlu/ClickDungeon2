using System;
using UnityEngine;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.Assets
{
    /// <summary>
    /// Resolves high-fidelity hero identity variants while retaining the legacy class/core sprite as
    /// a safe fallback. Gameplay class IDs remain stable and separate from presentation hero IDs.
    /// </summary>
    public static class HeroPresentationAssets
    {
        public static Sprite Core(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            assets?.SpriteFor(LegacyClassBaseId(heroClass));

        public static Sprite Master(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, StandardHeroId(heroClass), heroClass, "master");

        public static Sprite Gameplay(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, StandardHeroId(heroClass), heroClass, "gameplay");

        public static Sprite Portrait(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, StandardHeroId(heroClass), heroClass, "portrait");

        public static Sprite Roster(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, StandardHeroId(heroClass), heroClass, "roster");

        public static Sprite Idle(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, StandardHeroId(heroClass), heroClass, "idle");

        public static Sprite Attack(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, StandardHeroId(heroClass), heroClass, "attack");

        public static Sprite Hit(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, StandardHeroId(heroClass), heroClass, "hit");

        public static Sprite Victory(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, StandardHeroId(heroClass), heroClass, "victory");

        public static Sprite Defeat(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, StandardHeroId(heroClass), heroClass, "defeat");

        public static Sprite Master(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass) =>
            Variant(assets, heroId, fallbackClass, "master");

        public static Sprite Gameplay(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass) =>
            Variant(assets, heroId, fallbackClass, "gameplay");

        public static Sprite Portrait(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass) =>
            Variant(assets, heroId, fallbackClass, "portrait");

        public static Sprite Roster(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass) =>
            Variant(assets, heroId, fallbackClass, "roster");

        public static Sprite Idle(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass) =>
            Variant(assets, heroId, fallbackClass, "idle");

        public static Sprite Attack(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass) =>
            Variant(assets, heroId, fallbackClass, "attack");

        public static Sprite Hit(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass) =>
            Variant(assets, heroId, fallbackClass, "hit");

        public static Sprite Victory(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass) =>
            Variant(assets, heroId, fallbackClass, "victory");

        public static Sprite Defeat(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass) =>
            Variant(assets, heroId, fallbackClass, "defeat");

        public static string BaseId(HeroClassId heroClass) => HeroBaseId(StandardHeroId(heroClass));

        public static string HeroBaseId(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId)) return string.Empty;
            return "hero." + heroId.Trim().ToLowerInvariant();
        }

        public static string StandardHeroId(HeroClassId heroClass)
        {
            switch (heroClass)
            {
                case HeroClassId.Knight: return "ironheart";
                case HeroClassId.Ranger: return "windsong";
                case HeroClassId.Thief: return "shadowcut";
                case HeroClassId.Wizard: return "emberwisp";
                default: return heroClass.ToString().ToLowerInvariant();
            }
        }

        private static string LegacyClassBaseId(HeroClassId heroClass) =>
            "hero." + heroClass.ToString().ToLowerInvariant();

        private static Sprite Variant(PresentationAssetDatabase assets, string heroId, HeroClassId fallbackClass, string variant)
        {
            if (assets == null || string.IsNullOrWhiteSpace(variant)) return null;

            string identityBase = HeroBaseId(heroId);
            string classBase = LegacyClassBaseId(fallbackClass);
            string suffix = "." + variant.Trim().ToLowerInvariant();

            if (!string.IsNullOrEmpty(identityBase))
            {
                Sprite dedicated = assets.SpriteFor(identityBase + suffix) ?? assets.SpriteFor(identityBase);
                if (dedicated != null) return dedicated;
            }

            return assets.SpriteFor(classBase + suffix) ?? assets.SpriteFor(classBase);
        }
    }
}
