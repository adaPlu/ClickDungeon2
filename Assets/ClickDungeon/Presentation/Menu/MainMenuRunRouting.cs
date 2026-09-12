using ClickDungeon.Simulation.Model;

namespace ClickDungeon.Presentation.Menu
{
    public enum MainMenuRunAction
    {
        NewRun,
        ContinueRun
    }

    public static class MainMenuRunRouting
    {
        public static MainMenuRunAction PlayAction => MainMenuRunAction.NewRun;

        public static bool CanContinue(RunState activeRun)
        {
            return activeRun!=null;
        }
    }
}
