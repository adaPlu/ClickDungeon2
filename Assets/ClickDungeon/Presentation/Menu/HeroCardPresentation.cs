using System;
using System.Collections.Generic;
using ClickDungeon.Application.Heroes;
using ClickDungeon.Simulation.Content;

namespace ClickDungeon.Presentation.Menu
{
    public sealed class HeroCardDescriptor
    {
        public HeroCardDescriptor(string heroId,string displayName,string classLabel,string badge,string[] spriteKeys)
            : this(heroId,displayName,classLabel,badge,spriteKeys,0,0,0,string.Empty,string.Empty,Array.Empty<string>())
        {
        }

        public HeroCardDescriptor(string heroId,string displayName,string classLabel,string badge,string[] spriteKeys,int baseHp,int baseAttack,int baseDefense,string gameplayIdentity,string boardPassive,string[] abilityIds)
        {
            HeroId=heroId??string.Empty;
            DisplayName=displayName??string.Empty;
            ClassLabel=classLabel??string.Empty;
            Badge=badge??string.Empty;
            SpriteKeys=spriteKeys??Array.Empty<string>();
            BaseHp=baseHp;
            BaseAttack=baseAttack;
            BaseDefense=baseDefense;
            GameplayIdentity=gameplayIdentity??string.Empty;
            BoardPassive=boardPassive??string.Empty;
            AbilityIds=abilityIds??Array.Empty<string>();
        }

        public string HeroId { get; }
        public string DisplayName { get; }
        public string ClassLabel { get; }
        public string Badge { get; }
        public string[] SpriteKeys { get; }
        public int BaseHp { get; }
        public int BaseAttack { get; }
        public int BaseDefense { get; }
        public string GameplayIdentity { get; }
        public string BoardPassive { get; }
        public string[] AbilityIds { get; }
    }

    public static class HeroCardPresentation
    {
        private static readonly string[] SelectionHeroIds=
        {
            "ironheart",
            "clickington",
            "windsong",
            "shadowcut",
            "emberwisp",
            "lightbringer",
            "rageclaw",
            "gearspark",
            "dawnward"
        };

        public static IReadOnlyList<string> SelectionOrder => SelectionHeroIds;

        public static HeroCardDescriptor Describe(HeroIdentityDefinition hero)
        {
            if(hero==null)throw new ArgumentNullException(nameof(hero));
            string heroPrefix="hero."+hero.HeroId.ToLowerInvariant();
            return new HeroCardDescriptor(
                hero.HeroId,
                hero.DisplayName,
                hero.ClassId.ToString().ToUpperInvariant(),
                string.IsNullOrEmpty(hero.CampaignId)?string.Empty:"STORY CAMPAIGN",
                new[]{heroPrefix+".roster",heroPrefix+".portrait",heroPrefix+".master",heroPrefix+".gameplay"});
        }

        public static HeroCardDescriptor Describe(HeroIdentityDefinition hero,HeroDefinition mechanics)
        {
            if(hero==null)throw new ArgumentNullException(nameof(hero));
            if(mechanics==null)throw new ArgumentNullException(nameof(mechanics));
            if(mechanics.ClassId!=hero.ClassId)throw new ArgumentException($"Hero '{hero.HeroId}' is {hero.ClassId} but mechanics are {mechanics.ClassId}.",nameof(mechanics));
            string heroPrefix="hero."+hero.HeroId.ToLowerInvariant();
            return new HeroCardDescriptor(
                hero.HeroId,
                hero.DisplayName,
                hero.ClassId.ToString().ToUpperInvariant(),
                string.IsNullOrEmpty(hero.CampaignId)?string.Empty:"STORY CAMPAIGN",
                new[]{heroPrefix+".master",heroPrefix+".gameplay",heroPrefix+".roster",heroPrefix+".portrait"},
                mechanics.BaseHp,
                mechanics.BaseAttack,
                mechanics.BaseDefense,
                mechanics.Identity,
                mechanics.BoardPassive,
                mechanics.AbilityIds);
        }

        public static int WrapSelectionIndex(int index)
        {
            int count=SelectionHeroIds.Length;
            int wrapped=index%count;
            return wrapped<0?wrapped+count:wrapped;
        }

        public static HeroIdentityDefinition SelectionHeroAt(int index)
        {
            string heroId=SelectionHeroIds[WrapSelectionIndex(index)];
            foreach(var hero in HeroIdentityCatalog.All)
                if(string.Equals(hero.HeroId,heroId,StringComparison.OrdinalIgnoreCase))
                    return hero;
            throw new InvalidOperationException($"Approved hero selection identity '{heroId}' is not registered.");
        }
    }
}
