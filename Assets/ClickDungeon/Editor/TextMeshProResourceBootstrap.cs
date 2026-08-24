#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ClickDungeon.EditorTools
{
    public static class TextMeshProResourceBootstrap
    {
        public const string SettingsPath="Assets/TextMesh Pro/Resources/TMP Settings.asset";

        public static void Ensure()
        {
            if(File.Exists(SettingsPath))return;

            Type utilitiesType=AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeTypes)
                .FirstOrDefault(t=>string.Equals(t.FullName,"TMPro.TMP_PackageUtilities",StringComparison.Ordinal));
            MethodInfo importMethod=utilitiesType?.GetMethod("ImportProjectResourcesMenu",BindingFlags.Public|BindingFlags.Static);
            if(importMethod==null)
                throw new InvalidOperationException("TMP Essential Resources are missing and TMP_PackageUtilities.ImportProjectResourcesMenu could not be located. Import TMP Essential Resources before building.");

            importMethod.Invoke(null,null);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            if(!File.Exists(SettingsPath))
                throw new InvalidOperationException($"TMP Essential Resources import completed without creating {SettingsPath}.");

            Debug.Log($"Imported TMP Essential Resources at {SettingsPath}.");
        }

        private static Type[] SafeTypes(Assembly assembly)
        {
            try{return assembly.GetTypes();}
            catch(ReflectionTypeLoadException ex){return ex.Types.Where(t=>t!=null).ToArray();}
        }
    }
}
#endif
