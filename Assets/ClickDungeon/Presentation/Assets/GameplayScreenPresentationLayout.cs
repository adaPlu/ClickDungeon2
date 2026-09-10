namespace ClickDungeon.Presentation.Assets
{
    /// <summary>
    /// Pure normalized screen-layout contract for the live gameplay presentation.
    /// Coordinates are expressed inside the safe-area root and intentionally contain no Unity types
    /// so the hierarchy contract can be verified by the engine-free presentation test harness.
    /// </summary>
    public readonly struct NormalizedRegion
    {
        public NormalizedRegion(float minX,float minY,float maxX,float maxY)
        {
            MinX=minX;MinY=minY;MaxX=maxX;MaxY=maxY;
        }

        public float MinX { get; }
        public float MinY { get; }
        public float MaxX { get; }
        public float MaxY { get; }
        public float Width => MaxX-MinX;
        public float Height => MaxY-MinY;
    }

    public readonly struct GameplayScreenLayout
    {
        public GameplayScreenLayout(NormalizedRegion hud,NormalizedRegion board,NormalizedRegion actionBar,NormalizedRegion footer)
        {
            Hud=hud;Board=board;ActionBar=actionBar;Footer=footer;
        }

        public NormalizedRegion Hud { get; }
        public NormalizedRegion Board { get; }
        public NormalizedRegion ActionBar { get; }
        public NormalizedRegion Footer { get; }
    }

    public static class GameplayScreenPresentationLayout
    {
        // Landscape follows the approved reference hierarchy: thin HUD strip, dominant centered board,
        // large action row, persistent footer navigation.
        public static readonly GameplayScreenLayout Landscape=new GameplayScreenLayout(
            new NormalizedRegion(0f,.88f,1f,1f),
            new NormalizedRegion(.18f,.22f,.82f,.88f),
            new NormalizedRegion(.12f,.08f,.88f,.22f),
            new NormalizedRegion(0f,0f,1f,.08f));

        // Portrait preserves the same information order while dedicating most vertical space to the board.
        public static readonly GameplayScreenLayout Portrait=new GameplayScreenLayout(
            new NormalizedRegion(0f,.82f,1f,1f),
            new NormalizedRegion(.04f,.20f,.96f,.82f),
            new NormalizedRegion(0f,.08f,1f,.20f),
            new NormalizedRegion(0f,0f,1f,.08f));
    }
}
