namespace UniGame.UniBuild.Editor.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Editor;
    using global::Editor.Tools;
    using Inspector;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if TRI_INSPECTOR
    using TriInspector;
#endif

    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    [BuildCommandMetadata(
        displayName: "Apply Scripting Define Symbols",
        description: "Manages conditional compilation symbols for different build targets and configurations, enabling platform-specific code compilation.",
        category: "Build Configuration"
    )]
    public class ApplyScriptingDefineSymbolsCommand : SerializableBuildCommand
    {
        private const string DefinesSeparator = ";";

        [SerializeField]
        public string definesKey = "-defineValues";

        [SerializeField]
        public List<string> defaultDefines = new List<string>();

        [SerializeField]
        public List<string> removeDefines = new List<string>();

        public override void Execute(IUniBuilderConfiguration configuration)
        {
            if (!configuration.Arguments.GetStringValue(definesKey, out var defineValues))
            {
                defineValues = string.Empty;
            }

            Execute(defineValues);
        }

        public void Execute(string defineValues)
        {
            EditorSettingsUtility.ApplyDefines(defaultDefines, removeDefines, defineValues);
        }

#if ODIN_INSPECTOR || TRI_INSPECTOR
        [Button]
#endif
        public void Execute() => Execute(string.Empty);
    }
}