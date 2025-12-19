using System.Collections.Generic;

namespace UniGame.UniBuild.Editor.Abstract
{
    public interface IBuildCommands
    {
        IEnumerable<IUnityBuildCommand> Commands { get; }
        
    }
}