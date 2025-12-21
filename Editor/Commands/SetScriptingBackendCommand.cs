namespace UniGame.UniBuild.Editor.Commands
{
    using System;
    using Editor;
    using Inspector;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;

    /// <summary>
    /// Sets the scripting backend (Mono, IL2CPP) for the build
    /// </summary>
    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    [BuildCommandMetadata(
        displayName: "Set Scripting Backend",
        description: "Configures the scripting backend engine (Mono2x or IL2CPP) used for compilation, affecting performance and compatibility.",
        category: "Build Configuration"
    )]
    public class SetScriptingBackendCommand : SerializableBuildCommand
    {
        [SerializeField]
        private string l2cppEnabled = "-l2cppEnabled";

        private ScriptingImplementation _defaultBackend = ScriptingImplementation.Mono2x;

        public override void Execute(IUniBuilderConfiguration configuration)
        {
            var arguments = configuration.Arguments;
            var buildParameters = configuration.BuildParameters;
            
            var scriptingBackend = arguments.Contains(l2cppEnabled) ? 
                ScriptingImplementation.IL2CPP : 
                _defaultBackend;
            
#if FORCE_MONO
            scriptingBackend = ScriptingImplementation.Mono2x;
#elif FORCE_IL2CPP
            scriptingBackend = ScriptingImplementation.IL2CPP;
#endif
            
            switch (buildParameters.buildTargetGroup) {
                case BuildTargetGroup.Standalone:
                case BuildTargetGroup.Android:
                    PlayerSettings.SetScriptingBackend(buildParameters.buildTargetGroup,scriptingBackend);
                    Debug.Log($"Set ScriptingBackend: {scriptingBackend}");
                    return;
            }
            
            
        }
    }
}
