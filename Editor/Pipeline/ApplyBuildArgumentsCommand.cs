namespace UniModules.UniGame.UniBuild
{
    using System;
    using global::UniGame.UniBuild.Editor;
    using UnityEngine.Scripting.APIUpdating;


    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    public class ApplyBuildArgumentsCommand : SerializableBuildCommand
    {        
        public bool logArguments = true;

        public ArgumentsMap argumentsMap = new ArgumentsMap();

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            var arguments = buildParameters.Arguments;
            if (arguments == null) return;

            foreach (var argPair in argumentsMap.arguments)
            {
                if(logArguments)
                    BuildLogger.Log($"\n\t\tBUILD ARG: {argPair.Key} : {argPair.Value}");
                arguments.SetValue(argPair.Key, argPair.Value.Value);
            }
        }
    }
}