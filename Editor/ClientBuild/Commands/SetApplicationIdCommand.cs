namespace UniGame.UniBuild.Editor.Commands
{
    using UnityEditor;
    
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if TRI_INSPECTOR
    using TriInspector;
#endif
    
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
