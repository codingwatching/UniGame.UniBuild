using System;
using System.Collections.Generic;

namespace UniGame.UniBuild.Editor.Commands
{
    using Editor;

    [Serializable]
    public class SharedCommands : UnityBuildCommand
    {
        
        public List<BuildCommandStep> commands = new();

        public override void Execute(IUniBuilderConfiguration configuration)
        {
            foreach (var command in commands)
            {
                foreach (var buildCommand in command.GetCommands())
                {
                    if (!buildCommand.IsActive)
                    {
                        continue;
                    }
                    buildCommand.Execute(configuration);
                }
            }
        }
    }
}
