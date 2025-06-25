namespace UniGame.UniBuild.Editor.Commands
{
    using System;
    using ClientBuild.BuildConfiguration;
    using Editor;
    using UnityEditor;
    using UnityEngine.Scripting.APIUpdating;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    
#elif TRI_INSPECTOR
    using TriInspector;
#endif
    
    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    public class ApplyWebGLSettingsCommand : SerializableBuildCommand
    {
        public WebGlBuildData webGlBuildData = new();
        
        public override void Execute(IUniBuilderConfiguration configuration)
        {
            Execute();
            configuration.BuildParameters
                .UpdateWebGLData(webGlBuildData);
        }

#if ODIN_INSPECTOR || TRI_INSPECTOR
        [Button]
#endif
        public void Execute()
        {
            UpdateWebGLData(webGlBuildData);
        }
        
        public void UpdateWebGLData(WebGlBuildData data)
        {
            PlayerSettings.WebGL.showDiagnostics = webGlBuildData.ShowDiagnostics;
            PlayerSettings.WebGL.compressionFormat = webGlBuildData.CompressionFormat;
            PlayerSettings.WebGL.memorySize = webGlBuildData.MaxMemorySize;
            PlayerSettings.WebGL.dataCaching = webGlBuildData.DataCaching;
            PlayerSettings.WebGL.debugSymbolMode = webGlBuildData.DebugSymbolMode;
            PlayerSettings.WebGL.exceptionSupport = webGlBuildData.ExceptionSupport;
            PlayerSettings.defaultWebScreenWidth = webGlBuildData.Resolution.x;
            PlayerSettings.defaultWebScreenHeight = webGlBuildData.Resolution.y;
            PlayerSettings.WebGL.linkerTarget = webGlBuildData.LinkerTarget;
            
#if UNITY_WEBGL
            UnityEditor.WebGL.UserBuildSettings.codeOptimization = data.CodeOptimization;
#endif
        }
    }
}