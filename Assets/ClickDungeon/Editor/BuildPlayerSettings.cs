#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using ClickDungeon.Application.Versioning;

namespace ClickDungeon.EditorTools
{
    public static class BuildPlayerSettings
    {
        public const string CompanyName="adaPlu";
        public const string ProductName="ClickDungeon";
        public const string ApplicationIdentifier="com.adaplu.clickdungeon";

        public static void Apply(BuildTarget target)
        {
            PlayerSettings.companyName=CompanyName;
            PlayerSettings.productName=ProductName;
            PlayerSettings.bundleVersion=GameVersionInfo.GameVersion;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;

            switch(target)
            {
                case BuildTarget.Android:
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,ApplicationIdentifier);
                    PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel23;
                    PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel36;
                    PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARMv7|AndroidArchitecture.ARM64;
                    PlayerSettings.Android.resizeableActivity=true;
                    break;
                case BuildTarget.iOS:
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,ApplicationIdentifier);
                    PlayerSettings.iOS.targetOSVersionString="13.0";
                    PlayerSettings.iOS.targetDevice=iOSTargetDevice.iPhoneAndiPad;
                    break;
            }
        }
    }
}
#endif
