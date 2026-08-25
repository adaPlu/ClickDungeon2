using System.Runtime.InteropServices;

namespace ClickDungeon.Application.Platform
{
    public enum PersistentDataSyncStatus
    {
        Idle = 0,
        Pending = 1,
        Succeeded = 2,
        Failed = 3
    }

    public static class PersistentDataSync
    {
        public static PersistentDataSyncStatus LastStatus { get; private set; } = PersistentDataSyncStatus.Idle;
        public static string LastRequestedAtUtc { get; private set; } = string.Empty;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void ClickDungeonSyncPersistentData();
        [DllImport("__Internal")] private static extern int ClickDungeonGetPersistentDataSyncStatus();

        public static void RequestSync()
        {
            LastRequestedAtUtc = System.DateTimeOffset.UtcNow.ToString("O");
            LastStatus = PersistentDataSyncStatus.Pending;
            ClickDungeonSyncPersistentData();
        }

        public static PersistentDataSyncStatus PollStatus()
        {
            LastStatus = (PersistentDataSyncStatus)ClickDungeonGetPersistentDataSyncStatus();
            return LastStatus;
        }
#else
        public static void RequestSync()
        {
            LastRequestedAtUtc = System.DateTimeOffset.UtcNow.ToString("O");
            LastStatus = PersistentDataSyncStatus.Succeeded;
        }

        public static PersistentDataSyncStatus PollStatus() { return LastStatus; }
#endif
    }
}
