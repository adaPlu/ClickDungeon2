using System;

namespace ClickDungeon.Presentation.Assets
{
    public readonly struct VerticalSliceSequenceRequirement
    {
        public VerticalSliceSequenceRequirement(string id, string filePrefix, int frameCount, int framesPerSecond)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            FilePrefix = filePrefix ?? throw new ArgumentNullException(nameof(filePrefix));
            FrameCount = frameCount;
            FramesPerSecond = framesPerSecond;
        }

        public string Id { get; }
        public string FilePrefix { get; }
        public int FrameCount { get; }
        public int FramesPerSecond { get; }
    }

    public static class VerticalSliceProductionArtContract
    {
        public static readonly string[] ActorIds =
        {
            "hero.clickington",
            "monster.goblin_raider",
            "monster.slime",
            "monster.mimic_chest",
            "boss.lord_blobert"
        };

        public static readonly VerticalSliceSequenceRequirement[] RequiredSequences =
        {
            new VerticalSliceSequenceRequirement("hero.clickington.attack", "anim_clickington_attack_", 5, 12),
            new VerticalSliceSequenceRequirement("monster.mimic_chest.reveal", "anim_mimic_chest_reveal_", 6, 12),
            new VerticalSliceSequenceRequirement("monster.mimic_chest.bite", "anim_mimic_chest_bite_", 5, 12),
            new VerticalSliceSequenceRequirement("boss.lord_blobert.attack", "anim_lord_blobert_attack_", 6, 12)
        };

        public static readonly string[] RequiredStaticIds =
        {
            "hero.clickington.master",
            "hero.clickington.gameplay",
            "hero.clickington.portrait",
            "hero.clickington.roster",
            "hero.clickington.idle",
            "hero.clickington.attack",
            "hero.clickington.hit",
            "hero.clickington.victory",
            "hero.clickington.defeat",
            "monster.goblin_raider.gameplay",
            "monster.goblin_raider.idle",
            "monster.goblin_raider.attack",
            "monster.goblin_raider.hit",
            "monster.goblin_raider.defeat",
            "monster.slime.gameplay",
            "monster.slime.idle",
            "monster.slime.attack",
            "monster.slime.hit",
            "monster.slime.defeat",
            "monster.mimic_chest.gameplay",
            "monster.mimic_chest.idle",
            "monster.mimic_chest.attack",
            "monster.mimic_chest.hit",
            "monster.mimic_chest.defeat",
            "boss.lord_blobert.gameplay",
            "boss.lord_blobert.idle",
            "boss.lord_blobert.attack",
            "boss.lord_blobert.hit",
            "boss.lord_blobert.defeat"
        };
    }
}
