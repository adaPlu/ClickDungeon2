using NUnit.Framework;
using ClickDungeon.Simulation;
using ClickDungeon.Simulation.Commands;
using ClickDungeon.Simulation.Generation;
using ClickDungeon.Simulation.Model;

public sealed class GameSessionTests
{
    [Test] public void NonAdjacentRevealIsRejected()
    {
        var g=new FloorGenerator(); var state=g.CreateNewRun(1,HeroClassId.Knight); var session=new GameSession(state,g);
        Assert.IsFalse(session.Apply(new RevealTileCommand(0)).Accepted);
    }
    [Test] public void InvalidNoProgressActionDoesNotAdvanceCommandNumber()
    {
        var g=new FloorGenerator(); var state=g.CreateNewRun(2,HeroClassId.Knight); var session=new GameSession(state,g); long before=state.CommandNumber;
        session.Apply(new MoveCommand(0));
        Assert.AreEqual(before,state.CommandNumber);
    }
}
