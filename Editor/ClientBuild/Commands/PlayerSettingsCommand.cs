using System;
using UniGame.UniBuild.Editor;
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
