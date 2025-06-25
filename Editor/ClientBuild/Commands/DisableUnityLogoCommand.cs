using UniGame.Core.Runtime;

namespace UniGame.UniBuild.Editor.Commands {
    using System;
    using Editor;
    using UnityEditor;
    using UnityEngine.Scripting.APIUpdating;

#if ODIN_INSPECTOR
     using Sirenix.OdinInspector;
#endif

#if TRI_INSPECTOR
    using TriInspector;
#endif
    
    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    public class DisableUnityLogoCommand : SerializableBuildCommand, ICommand
    {

        public bool enableSplashScreen = false;

        public bool showUnityLogo = false;
        
        public override void Execute(IUniBuilderConfiguration buildParameters) => Execute();

#if  ODIN_INSPECTOR || TRI_INSPECTOR
        [Button]
#endif
        public void Execute()
        {
            PlayerSettings.SplashScreen.show = enableSplashScreen;
            PlayerSettings.SplashScreen.showUnityLogo = showUnityLogo;
            AssetDatabase.SaveAssets();
        }
        
    }
}
