#if UNITY_EDITOR
using System;
using System.IO;
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

        public readonly struct AndroidSigningState
        {
            internal readonly bool UseCustomKeystore;
            internal readonly string KeystoreName;
            internal readonly string KeystorePass;
            internal readonly string KeyaliasName;
            internal readonly string KeyaliasPass;

            internal AndroidSigningState(bool useCustomKeystore,string keystoreName,string keystorePass,string keyaliasName,string keyaliasPass)
            {
                UseCustomKeystore=useCustomKeystore;
                KeystoreName=keystoreName;
                KeystorePass=keystorePass;
                KeyaliasName=keyaliasName;
                KeyaliasPass=keyaliasPass;
            }
        }

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

        public static AndroidSigningState ApplyAndroidSigningFromEnvironment()
        {
            var previous=new AndroidSigningState(
                PlayerSettings.Android.useCustomKeystore,
                PlayerSettings.Android.keystoreName,
                PlayerSettings.Android.keystorePass,
                PlayerSettings.Android.keyaliasName,
                PlayerSettings.Android.keyaliasPass);

            string path=Environment.GetEnvironmentVariable("CLICKDUNGEON_ANDROID_KEYSTORE_PATH");
            string storePassword=Environment.GetEnvironmentVariable("CLICKDUNGEON_ANDROID_KEYSTORE_PASSWORD");
            string alias=Environment.GetEnvironmentVariable("CLICKDUNGEON_ANDROID_KEY_ALIAS");
            string keyPassword=Environment.GetEnvironmentVariable("CLICKDUNGEON_ANDROID_KEY_PASSWORD");

            bool anyConfigured=!string.IsNullOrWhiteSpace(path)||!string.IsNullOrWhiteSpace(storePassword)||!string.IsNullOrWhiteSpace(alias)||!string.IsNullOrWhiteSpace(keyPassword);
            if(!anyConfigured)
            {
                PlayerSettings.Android.useCustomKeystore=false;
                Debug.LogWarning("Android release signing secrets are not configured; producing a debug-signed CI smoke bundle.");
                return previous;
            }

            if(string.IsNullOrWhiteSpace(path)||string.IsNullOrWhiteSpace(storePassword)||string.IsNullOrWhiteSpace(alias)||string.IsNullOrWhiteSpace(keyPassword)||!File.Exists(path))
                throw new InvalidOperationException("Android release signing environment is incomplete. Configure keystore bytes, store password, key alias, and key password together.");

            PlayerSettings.Android.useCustomKeystore=true;
            PlayerSettings.Android.keystoreName=Path.GetFullPath(path);
            PlayerSettings.Android.keystorePass=storePassword;
            PlayerSettings.Android.keyaliasName=alias;
            PlayerSettings.Android.keyaliasPass=keyPassword;
            Debug.Log("Android custom release signing enabled from CI environment.");
            return previous;
        }

        public static void RestoreAndroidSigning(AndroidSigningState state)
        {
            PlayerSettings.Android.useCustomKeystore=state.UseCustomKeystore;
            PlayerSettings.Android.keystoreName=state.KeystoreName;
            PlayerSettings.Android.keystorePass=state.KeystorePass;
            PlayerSettings.Android.keyaliasName=state.KeyaliasName;
            PlayerSettings.Android.keyaliasPass=state.KeyaliasPass;
        }
    }
}
#endif
