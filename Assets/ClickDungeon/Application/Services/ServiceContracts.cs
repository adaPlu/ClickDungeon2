using System;
using System.Collections.Generic;

namespace ClickDungeon.Application.Services
{
    public interface IAnalytics
    {
        void Track(string eventName,IReadOnlyDictionary<string,string> properties=null);
    }

    public interface IRemoteConfig
    {
        string Revision { get; }
        int GetInt(string key,int fallback);
        bool GetBool(string key,bool fallback);
        string GetString(string key,string fallback);
    }

    public interface IAdService
    {
        bool IsSupported { get; }
        void Initialize();
    }

    public interface IStoreService
    {
        bool IsSupported { get; }
        bool FullGameUnlocked { get; }
        void RefreshEntitlements(Action<bool> completed=null);
        void PurchaseFullGame(Action<bool,string> completed);
    }

    public interface ICloudSave
    {
        bool IsSupported { get; }
        void UploadSlot(int slot,string json,Action<bool,string> completed=null);
        void DownloadSlot(int slot,Action<bool,string> completed);
    }

    public interface IPlatformCapabilities
    {
        bool IsDesktop { get; }
        bool IsMobile { get; }
        bool IsWeb { get; }
        bool SupportsHaptics { get; }
        bool SupportsFileExport { get; }
    }
}
