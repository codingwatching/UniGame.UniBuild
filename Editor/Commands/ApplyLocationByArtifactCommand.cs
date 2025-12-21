namespace UniGame.UniBuild.Editor.Commands
{
    using System.IO;
    using global::UniGame.Core.Runtime.Extension;
    using UnityEditor;
    using System;
    using Editor;
    using Inspector;
    using UniModules;
    using UnityEngine.Scripting.APIUpdating;

    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    [BuildCommandMetadata(
        displayName: "Apply Build Location by Artifact",
        description: "Configures the build output directory path based on the artifact name and platform-specific conventions.",
        category: "Build Configuration"
    )]
    public class ApplyLocationByArtifactCommand : SerializableBuildCommand
    {

        public ArtifactLocationOption option = ArtifactLocationOption.Append;

        public bool useArtifactNameAsFolderPath = true;

        public string location;

        public bool appendBuildTarget = true;
        
        public override void Execute(IUniBuilderConfiguration configuration)
        {
            var buildParameters = configuration.BuildParameters;
            var resultLocation = buildParameters.outputFolder;
            var artifact = buildParameters.outputFile;
            var buildTarget = buildParameters.buildTarget;
            
            resultLocation =CreateArtifactLocation(artifact,resultLocation,buildTarget);
            buildParameters.outputFolder = resultLocation;
        }

        public string CreateArtifactLocation(string artifactName,string sourceLocation,BuildTarget buildTarget)
        {
            var artifact = Path
                .GetFileNameWithoutExtension(artifactName)
                .RemoveSpecialAndDotsCharacters()
                .RemoveWhiteSpaces();
            
            var locationPath = location
                .RemoveSpecialAndDotsCharacters()
                .RemoveWhiteSpaces();

            locationPath = option == ArtifactLocationOption.Replace
                ? locationPath
                : sourceLocation.CombinePath(locationPath);
            
            locationPath = appendBuildTarget ? locationPath.CombinePath(buildTarget.ToString()) : locationPath;
            locationPath = useArtifactNameAsFolderPath ? locationPath.CombinePath(artifact) : locationPath;
            return locationPath.CombinePath(string.Empty);
        }
    }

    public enum ArtifactLocationOption
    {
        Append,
        Replace
    }
}
