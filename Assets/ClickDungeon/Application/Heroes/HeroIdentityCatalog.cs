using System;
using System.Collections.Generic;
using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Application.Heroes
{
    public sealed class HeroIdentityDefinition
    {
        public HeroIdentityDefinition(string heroId,string displayName,HeroClassId classId,string campaignId="")
        {
            HeroId=heroId??throw new ArgumentNullException(nameof(heroId));
            DisplayName=displayName??throw new ArgumentNullException(nameof(displayName));
            ClassId=classId;
            CampaignId=campaignId??string.Empty;
        }

        public string HeroId { get; }
        public string DisplayName { get; }
        public HeroClassId ClassId { get; }
        public string CampaignId { get; }
    }

    public static class HeroIdentityCatalog
    {
        private static readonly HeroIdentityDefinition[] Definitions =
        {
            new HeroIdentityDefinition("ironheart","Ironheart",HeroClassId.Knight),
            new HeroIdentityDefinition("clickington","Sir Clickington",HeroClassId.Knight,"clickington_campaign"),
            new HeroIdentityDefinition("windsong","Windsong",HeroClassId.Ranger),
            new HeroIdentityDefinition("shadowcut","Shadowcut",HeroClassId.Thief),
            new HeroIdentityDefinition("emberwisp","Emberwisp",HeroClassId.Wizard)
        };

        private static readonly HashSet<string> VisualVariants = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "portrait","roster","select","gameplay","idle","attack","hit","victory","defeat"
        };

        public static IEnumerable<HeroIdentityDefinition> All => Definitions;

        public static IEnumerable<HeroIdentityDefinition> ForClass(HeroClassId classId)
        {
            for(int i=0;i<Definitions.Length;i++)
                if(Definitions[i].ClassId==classId)
                    yield return Definitions[i];
        }

        public static string ResolveHeroId(HeroClassId classId,string requestedHeroId)
        {
            var requested=Find(requestedHeroId);
            if(requested!=null&&requested.ClassId==classId)return requested.HeroId;
            return StandardHeroId(classId);
        }

        public static HeroClassId ClassForHero(string heroId)
        {
            var definition=Find(heroId);
            if(definition==null)throw new ArgumentException($"Unknown hero identity '{heroId}'.",nameof(heroId));
            return definition.ClassId;
        }

        public static string DisplayNameForHero(string heroId)
        {
            var definition=Find(heroId);
            return definition?.DisplayName??string.Empty;
        }

        public static string CampaignForHero(string heroId)
        {
            var definition=Find(heroId);
            return definition?.CampaignId??string.Empty;
        }

        public static string SelectionLabelForHero(string heroId)
        {
            var definition=Find(heroId);
            if(definition==null)return string.Empty;
            string label=$"{definition.DisplayName} — {definition.ClassId}";
            return string.IsNullOrEmpty(definition.CampaignId)?label:label+" • Story Campaign";
        }

        public static string VisualAssetKeyForHero(string heroId,string variant)
        {
            var definition=Find(heroId);
            if(definition==null)throw new ArgumentException($"Unknown hero identity '{heroId}'.",nameof(heroId));
            if(string.IsNullOrWhiteSpace(variant)||!VisualVariants.Contains(variant))throw new ArgumentException($"Unknown hero visual variant '{variant}'.",nameof(variant));
            return $"hero.{definition.HeroId}.{variant.ToLowerInvariant()}";
        }

        public static string StandardHeroId(HeroClassId classId)
        {
            switch(classId)
            {
                case HeroClassId.Knight:return "ironheart";
                case HeroClassId.Ranger:return "windsong";
                case HeroClassId.Thief:return "shadowcut";
                case HeroClassId.Wizard:return "emberwisp";
                default:throw new ArgumentOutOfRangeException(nameof(classId),classId,"Unsupported gameplay class.");
            }
        }

        private static HeroIdentityDefinition Find(string heroId)
        {
            if(string.IsNullOrWhiteSpace(heroId))return null;
            for(int i=0;i<Definitions.Length;i++)
                if(string.Equals(Definitions[i].HeroId,heroId,StringComparison.OrdinalIgnoreCase))
                    return Definitions[i];
            return null;
        }
    }
}
