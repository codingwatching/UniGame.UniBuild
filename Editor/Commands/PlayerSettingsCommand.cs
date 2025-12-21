using System;
using UniGame.UniBuild.Editor;
using UniGame.UniBuild.Editor.Inspector;
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
[BuildCommandMetadata(
    displayName: "Player Settings",
    description: "Configures player build settings including incremental IL2CPP compilation for optimized build times.",
    category: "Player Settings"
)]
public class PlayerSettingsCommand : SerializableBuildCommand
{
    public bool setIncrementalIl2CppBuild = true;
    
    public override void Execute(IUniBuilderConfiguration buildParameters)
    {
        Execute(buildParameters.BuildParameters.buildTargetGroup);
    }

#if  ODIN_INSPECTOR || TRI_INSPECTOR
    [Button]
#endif
    public void Execute(BuildTargetGroup targetGroup)
    {
        PlayerSettings.SetIncrementalIl2CppBuild(targetGroup,setIncrementalIl2CppBuild);
    }
}
