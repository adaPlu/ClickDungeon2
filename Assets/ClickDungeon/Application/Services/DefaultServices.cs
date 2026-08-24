using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClickDungeon.Application.Services
{
    public sealed class NullAnalytics : IAnalytics
    {
        public void Track(string eventName,IReadOnlyDictionary<string,string> properties=null) { }
    }

    /// <summary>Run-pinned local configuration. Network-backed config can replace this later without touching simulation rules.</summary>
    public sealed class LocalRemoteConfig : IRemoteConfig
    {
        private readonly Dictionary<string,string> _values;
        public string Revision { get; }
        public LocalRemoteConfig(string revision="local-1",Dictionary<string,string> values=null){Revision=revision;_values=values??new Dictionary<string,string>(StringComparer.Ordinal);}
        public int GetInt(string key,int fallback)=>_values.TryGetValue(key,out var v)&&int.TryParse(v,out var n)?n:fallback;
        public bool GetBool(string key,bool fallback)=>_values.TryGetValue(key,out var v)&&bool.TryParse(v,out var n)?n:fallback;
        public string GetString(string key,string fallback)=>_values.TryGetValue(key,out var v)?v:fallback;
    }

    /// <summary>Ads are intentionally unsupported in the launch design.</summary>
    public sealed class NullAdService : IAdService
    {
        public bool IsSupported=>false;
        public void Initialize() { }
    }

    /// <summary>
    /// Development entitlement adapter. Production mobile builds replace this with Apple/Google
    /// one-time full-game purchase implementations. Steam/desktop is considered entitled by purchase.
    /// </summary>
    public sealed class LocalEntitlementStore : IStoreService
    {
        private const string Key="cd2.full_game_unlocked";
        private readonly bool _desktopEntitled;
        public LocalEntitlementStore(){_desktopEntitled=UnityEngine.Application.isEditor||UnityEngine.Application.platform==RuntimePlatform.WindowsPlayer||UnityEngine.Application.platform==RuntimePlatform.OSXPlayer||UnityEngine.Application.platform==RuntimePlatform.LinuxPlayer;}
        public bool IsSupported=>true;
        public bool FullGameUnlocked=>_desktopEntitled||PlayerPrefs.GetInt(Key,0)==1;
        public void RefreshEntitlements(Action<bool> completed=null)=>completed?.Invoke(FullGameUnlocked);
        public void PurchaseFullGame(Action<bool,string> completed){if(_desktopEntitled){completed?.Invoke(true,"desktop_entitled");return;}PlayerPrefs.SetInt(Key,1);PlayerPrefs.Save();completed?.Invoke(true,"development_unlock");}
    }

    /// <summary>Cloud save is deliberately local-first and disabled until a real provider is selected.</summary>
    public sealed class NullCloudSave : ICloudSave
    {
        public bool IsSupported=>false;
        public void UploadSlot(int slot,string json,Action<bool,string> completed=null)=>completed?.Invoke(false,"cloud_save_not_configured");
        public void DownloadSlot(int slot,Action<bool,string> completed)=>completed?.Invoke(false,string.Empty);
    }

    public sealed class UnityPlatformCapabilities : IPlatformCapabilities
    {
        public bool IsWeb=>UnityEngine.Application.platform==RuntimePlatform.WebGLPlayer;
        public bool IsMobile=>UnityEngine.Application.isMobilePlatform;
        public bool IsDesktop=>!IsWeb&&!IsMobile;
        public bool SupportsHaptics=>IsMobile;
        public bool SupportsFileExport=>!IsWeb;
    }

    public sealed class ServiceRegistry
    {
        public IAnalytics Analytics { get; }
        public IRemoteConfig RemoteConfig { get; }
        public IAdService Ads { get; }
        public IStoreService Store { get; }
        public ICloudSave CloudSave { get; }
        public IPlatformCapabilities Platform { get; }

        public ServiceRegistry(IAnalytics analytics=null,IRemoteConfig remoteConfig=null,IAdService ads=null,IStoreService store=null,ICloudSave cloudSave=null,IPlatformCapabilities platform=null)
        {
            Analytics=analytics??new NullAnalytics();RemoteConfig=remoteConfig??new LocalRemoteConfig();Ads=ads??new NullAdService();Store=store??new LocalEntitlementStore();CloudSave=cloudSave??new NullCloudSave();Platform=platform??new UnityPlatformCapabilities();
        }
    }
}
