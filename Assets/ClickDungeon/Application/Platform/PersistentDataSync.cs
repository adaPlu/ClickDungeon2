using System.Runtime.InteropServices;

namespace ClickDungeon.Application.Platform
{
    public static class PersistentDataSync
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void ClickDungeonSyncPersistentData();
        public static void RequestSync() { ClickDungeonSyncPersistentData(); }
#else
        public static void RequestSync() { }
#endif
    }
}
