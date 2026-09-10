using System;
using UnityEngine;
using ClickDungeon.Application.Heroes;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.Assets
{
    /// <summary>
    /// Resolves high-fidelity hero identity variants. Gameplay class IDs remain stable and separate
    /// from presentation hero IDs; identity-aware calls never borrow another hero's mechanical-class art.
    /// </summary>
    public static class HeroPresentationAssets
    {
        public static Sprite Core(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            assets?.SpriteFor("hero."+heroClass.ToString().ToLowerInvariant());

        public static Sprite Master(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, HeroIdentityCatalog.StandardHeroId(heroClass), "master");

        public static Sprite Gameplay(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, HeroIdentityCatalog.StandardHeroId(heroClass), "gameplay");

        public static Sprite Portrait(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, HeroIdentityCatalog.StandardHeroId(heroClass), "portrait");

        public static Sprite Roster(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, HeroIdentityCatalog.StandardHeroId(heroClass), "roster");

        public static Sprite Idle(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, HeroIdentityCatalog.StandardHeroId(heroClass), "idle");

        public static Sprite Attack(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, HeroIdentityCatalog.StandardHeroId(heroClass), "attack");

        public static Sprite Hit(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, HeroIdentityCatalog.StandardHeroId(heroClass), "hit");

        public static Sprite Victory(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, HeroIdentityCatalog.StandardHeroId(heroClass), "victory");

        public static Sprite Defeat(PresentationAssetDatabase assets, HeroClassId heroClass) =>
            Variant(assets, HeroIdentityCatalog.StandardHeroId(heroClass), "defeat");

        // Preserve the public bridge signature during this remaster, but never use the class argument
        // to substitute art for a named identity.
        public static Sprite Master(PresentationAssetDatabase assets, string heroId, HeroClassId ignoredClass) => Variant(assets, heroId, "master");
        public static Sprite Gameplay(PresentationAssetDatabase assets, string heroId, HeroClassId ignoredClass) => Variant(assets, heroId, "gameplay");
        public static Sprite Portrait(PresentationAssetDatabase assets, string heroId, HeroClassId ignoredClass) => Variant(assets, heroId, "portrait");
        public static Sprite Roster(PresentationAssetDatabase assets, string heroId, HeroClassId ignoredClass) => Variant(assets, heroId, "roster");
        public static Sprite Idle(PresentationAssetDatabase assets, string heroId, HeroClassId ignoredClass) => Variant(assets, heroId, "idle");
        public static Sprite Attack(PresentationAssetDatabase assets, string heroId, HeroClassId ignoredClass) => Variant(assets, heroId, "attack");
        public static Sprite Hit(PresentationAssetDatabase assets, string heroId, HeroClassId ignoredClass) => Variant(assets, heroId, "hit");
        public static Sprite Victory(PresentationAssetDatabase assets, string heroId, HeroClassId ignoredClass) => Variant(assets, heroId, "victory");
        public static Sprite Defeat(PresentationAssetDatabase assets, string heroId, HeroClassId ignoredClass) => Variant(assets, heroId, "defeat");

        public static string BaseId(HeroClassId heroClass) => HeroBaseId(HeroIdentityCatalog.StandardHeroId(heroClass));

        public static string HeroBaseId(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId)) return string.Empty;
            return "hero." + heroId.Trim().ToLowerInvariant();
        }

        public static string StandardHeroId(HeroClassId heroClass) => HeroIdentityCatalog.StandardHeroId(heroClass);

        private static Sprite Variant(PresentationAssetDatabase assets, string heroId, string variant)
        {
            if (assets == null || string.IsNullOrWhiteSpace(heroId) || string.IsNullOrWhiteSpace(variant)) return null;
            return assets.SpriteFor(HeroBaseId(heroId) + "." + variant.Trim().ToLowerInvariant());
        }
    }
}
