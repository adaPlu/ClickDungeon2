namespace ClickDungeon.Presentation
{
    /// <summary>
    /// Disambiguates UnityEngine.Application from the ClickDungeon.Application
    /// namespace inside presentation code.
    /// </summary>
    internal static class Application
    {
        public static string dataPath => UnityEngine.Application.dataPath;
    }
}
