namespace UniGame.UniBuild.Editor.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using Object = UnityEngine.Object;

    public static class AssetEditorTools
    {
        /// <summary>
        /// Finds and loads all assets of type T in the project (including subfolders).
        /// Works for ScriptableObject assets, prefabs with components, etc.
        /// </summary>
        public static List<T> GetAssets<T>() where T : Object
        {
            var results = new List<T>();

            // For most Unity asset types this is enough.
            // Note: For Components, this finds prefabs that have that component attached.
            var filter = $"t:{typeof(T).Name}";
            string[] guids = AssetDatabase.FindAssets(filter);

            results.Capacity = guids.Length;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                    continue;

                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    results.Add(asset);
            }

            return results;
        }
        
        public static string GetGUID(this Object asset)
        {
            if (asset == null) return string.Empty;

            var path = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrEmpty(path) 
                ? string.Empty 
                : AssetDatabase.AssetPathToGUID(path);
        }

        /// <summary>
        /// Overload: search only inside specific folders (e.g. "Assets/GameData").
        /// </summary>
        public static List<T> GetAssets<T>(params string[] folders) where T : Object
        {
            var results = new List<T>();

            var filter = $"t:{typeof(T).Name}";
            string[] guids = (folders != null && folders.Length > 0)
                ? AssetDatabase.FindAssets(filter, folders)
                : AssetDatabase.FindAssets(filter);

            results.Capacity = guids.Length;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                    continue;

                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    results.Add(asset);
            }

            return results;
        }
        
        public static bool OpenScript(this Type type, params string[] folders)
        {
            var asset = GetScriptAsset(type, folders);
            if (asset == null)
                return false;
            return AssetDatabase.OpenAsset(asset.GetInstanceID(), 0, 0);
        }

        public static MonoScript GetScriptAsset(this Type type, params string[] folders)
        {
            var typeName = type.Name;
            var filter   = $"t:script {typeName}";

            var assets = AssetDatabase.FindAssets(filter, folders);
            var assetPath = string.Empty;

            foreach (var s in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(s);
                var scriptTypeName = Path.GetFileNameWithoutExtension(path);
                if(!string.Equals(typeName, scriptTypeName, StringComparison.OrdinalIgnoreCase))
                    continue;
                assetPath = path;
                break;
            }

            if (string.IsNullOrEmpty(assetPath))
                return null;

            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            return asset;
        }
    }
}