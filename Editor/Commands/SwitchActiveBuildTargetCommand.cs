namespace UniGame.UniBuild.Editor.Commands
{
    using System;
    using Editor;
    using UnityEditor;
    using UnityEngine.Scripting.APIUpdating;

    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    public class SwitchActiveBuildTargetCommand : SerializableBuildCommand
    {
        
        public BuildTargetGroup BuildTargetGroup = BuildTargetGroup.Android;
        public BuildTarget BuildTarget = BuildTarget.Android;
        
        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup,BuildTarget);
        }
    }
}
