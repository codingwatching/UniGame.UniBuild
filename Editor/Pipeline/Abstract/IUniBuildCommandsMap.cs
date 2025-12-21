using System;
using System.Collections.Generic;

namespace UniGame.UniBuild.Editor.Abstract
{
    using ClientBuild.BuildConfiguration;

    public interface IUniBuildCommandsMap : 
        IUnityBuildCommandValidator
    {
        public bool PlayerBuildEnabled { get; }
  
        UniBuildConfigurationData BuildData { get; }
        
        IEnumerable<IUnityBuildCommand> PreBuildCommands { get; }

        IEnumerable<IUnityBuildCommand> PostBuildCommands  { get; }
        
        IEnumerable<T> LoadCommands<T>(Func<T,bool> filter = null) where T : IUnityBuildCommand;
    }
}