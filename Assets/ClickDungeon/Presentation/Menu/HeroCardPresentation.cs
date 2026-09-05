using System;
using ClickDungeon.Application.Heroes;

namespace ClickDungeon.Presentation.Menu
{
    public sealed class HeroCardDescriptor
    {
        public HeroCardDescriptor(string heroId,string displayName,string classLabel,string badge,string[] spriteKeys)
        {
            HeroId=heroId??string.Empty;
            DisplayName=displayName??string.Empty;
            ClassLabel=classLabel??string.Empty;
            Badge=badge??string.Empty;
            SpriteKeys=spriteKeys??Array.Empty<string>();
        }

        public string HeroId { get; }
        public string DisplayName { get; }
        public string ClassLabel { get; }
        public string Badge { get; }
        public string[] SpriteKeys { get; }
    }

    public static class HeroCardPresentation
    {
        public static HeroCardDescriptor Describe(HeroIdentityDefinition hero)
        {
            if(hero==null)throw new ArgumentNullException(nameof(hero));
            string heroPrefix="hero."+hero.HeroId.ToLowerInvariant();
            string classFallback="hero."+hero.ClassId.ToString().ToLowerInvariant();
            return new HeroCardDescriptor(
                hero.HeroId,
                hero.DisplayName,
                hero.ClassId.ToString().ToUpperInvariant(),
                string.IsNullOrEmpty(hero.CampaignId)?string.Empty:"STORY CAMPAIGN",
                new[]{heroPrefix+".roster",heroPrefix+".portrait",heroPrefix+".select",classFallback});
        }
    }
}
