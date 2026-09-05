using NUnit.Framework;
using ClickDungeon.Application.Heroes;
using ClickDungeon.Presentation.Menu;
using ClickDungeon.Simulation.Model;

public sealed class HeroCardPresentationTests
{
    [Test]
    public void IronheartCardUsesIdentityArtBeforeKnightFallback()
    {
        var hero=new HeroIdentityDefinition("ironheart","Ironheart",HeroClassId.Knight);
        var card=HeroCardPresentation.Describe(hero);

        Assert.AreEqual("Ironheart",card.DisplayName);
        Assert.AreEqual("KNIGHT",card.ClassLabel);
        Assert.AreEqual(string.Empty,card.Badge);
        CollectionAssert.AreEqual(new[]{"hero.ironheart.roster","hero.ironheart.portrait","hero.ironheart.select","hero.knight"},card.SpriteKeys);
    }

    [Test]
    public void ClickingtonCardShowsStoryBadgeWithoutChangingKnightMechanics()
    {
        var hero=new HeroIdentityDefinition("clickington","Sir Clickington",HeroClassId.Knight,"clickington_campaign");
        var card=HeroCardPresentation.Describe(hero);

        Assert.AreEqual("Sir Clickington",card.DisplayName);
        Assert.AreEqual("KNIGHT",card.ClassLabel);
        Assert.AreEqual("STORY CAMPAIGN",card.Badge);
        CollectionAssert.AreEqual(new[]{"hero.clickington.roster","hero.clickington.portrait","hero.clickington.select","hero.knight"},card.SpriteKeys);
    }
}
