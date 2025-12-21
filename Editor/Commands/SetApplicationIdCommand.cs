namespace UniGame.UniBuild.Editor.Commands
{
    using Inspector;
    using UnityEditor;
    
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if TRI_INSPECTOR
    using TriInspector;
#endif
    
    [BuildCommandMetadata(
        displayName: "Set Application ID",
        description: "Sets the application identifier (bundle ID) for the build, typically in the format com.company.product.",
        category: "Player Settings"
    )]
    public class SetApplicationIdCommand : UnityBuildCommand
    {
        public string applicationId = "com.company.product";
        
        public override void Execute(IUniBuilderConfiguration configuration)
        {
            Execute();
        }

#if  ODIN_INSPECTOR || TRI_INSPECTOR
        [Button]
#endif
        public void Execute()
        {
            PlayerSettings.applicationIdentifier = applicationId;
        }
    }
}
