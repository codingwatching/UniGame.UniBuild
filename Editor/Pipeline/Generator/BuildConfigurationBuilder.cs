namespace UniGame.UniBuild.Editor
{
    using System.Collections.Generic;
    using Utils;
    using UnityEditor;
    using UnityEngine;

    public class BuildConfigurationBuilder
    {
        public const string GeneratedContentDefaultPath = "Assets/UniGame.Generated/";
        
        private static Dictionary<string, string> _fileContentCache = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetCache()
        {
            _fileContentCache.Clear();
        }
        

        private static string _cloudLocalPath = "UniBuild/Editor/" + CloudBuildMethodsGenerator.ClassFileName;
        private static string _cloudPath = FileUtils.Combine(GeneratedContentDefaultPath, _cloudLocalPath);

        private static string _menuScript = string.Empty;
        private static string _cloudScript = string.Empty;

        public static string BuildPath => FileUtils.Combine(GeneratedContentDefaultPath, "UniBuild/Editor/BuildMethods.cs");

        [MenuItem("UniGame/Build Pipeline/Rebuild Menu")]
        public static void RebuildMenuAction()
        {
            Rebuild(true);
        }

        public static void Rebuild(bool forceUpdate = false)
        {
            RebuildMenu(forceUpdate);
            RebuildCloudMethods(forceUpdate);
        }

        public static bool RebuildMenu(bool force = false)
        {
#if UNITY_CLOUD_BUILD
            return false;
#endif
            var generator = new BuildMenuGenerator();
            var script = generator.CreateBuilderScriptBody();
            var result = WriteUnityFile(script, BuildPath, force);

            return result;
        }

        public static bool RebuildCloudMethods(bool force = false)
        {
#if UNITY_CLOUD_BUILD
            return false;
#endif
            var cloudGenerator = new CloudBuildMethodsGenerator();
            var content = cloudGenerator.CreateCloudBuildMethods();
            var result = WriteUnityFile(content, _cloudPath, force);
            return result;
        }

        public static bool WriteUnityFile(string scriptValue, string path, bool force = false)
        {
            if (!_fileContentCache.TryGetValue(path, out var content))
            {
                var data = FileUtils.ReadContent(path, false);
                content = string.IsNullOrEmpty(data.content) ? string.Empty : data.content;
            }

            if (string.IsNullOrEmpty(scriptValue))
                return false;

            if (!force && scriptValue.Equals(content))
                return false;

            var result = FileUtils.WriteAssetsContent(path, scriptValue);
            if (result)
                _fileContentCache[path] = scriptValue;

            return result;
        }

    }
}