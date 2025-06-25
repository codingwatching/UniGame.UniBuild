namespace UniGame.UniBuild.Editor.Commands
{
    using System;
    using Editor;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;

#if ODIN_INSPECTOR
     using Sirenix.OdinInspector;
#endif

#if TRI_INSPECTOR
    using TriInspector;
#endif
    
    [Serializable]
    [CreateAssetMenu(menuName = "UniBuild/Commands/DefineSymbolsAssetCommand",fileName = nameof(DefineSymbolsAssetCommand))]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    public class DefineSymbolsAssetCommand : UnityBuildCommand
    {
        [SerializeField]
#if  ODIN_INSPECTOR || TRI_INSPECTOR
        [InlineProperty]
        [HideLabel]
#endif
        private ApplyScriptingDefineSymbolsCommand _command = new ApplyScriptingDefineSymbolsCommand();

        public override void Execute(IUniBuilderConfiguration configuration)
        {
            _command.Execute(configuration);
        }

#if ODIN_INSPECTOR || TRI_INSPECTOR
        [Button("Apply Defines")]
#endif
        public void Execute() => _command.Execute(string.Empty);
        
    }
}
