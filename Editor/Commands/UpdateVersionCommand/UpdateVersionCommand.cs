using System.IO;

namespace UniGame.UniBuild.Editor 
{
    using System;
    using System.Text;
    using global::UniGame.UniBuild.UpdateVersionCommand;
    using Inspector;
    using UnityEditor;
    using UnityEditor.Build.Profile;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;
    using Utils;

#if ODIN_INSPECTOR
     using Sirenix.OdinInspector;
#endif

#if TRI_INSPECTOR
    using TriInspector;
#endif
    
    /// <summary>
    /// Updates current project version with configurable build number increments and branch appending.
    /// </summary>
    [Serializable]
    [MovedFrom(sourceNamespace:"UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands")]
    [BuildCommandMetadata(
        displayName: "Update Version",
        description: "Updates the project build version number by incrementing it according to specified parameters. Supports appending git branch information and writing version to a file for tracking build history.",
        category: "Version Management"
    )]
    public class UpdateVersionCommand : SerializableBuildCommand
    {
        public int minBuildNumber = 0;
        
        public int incrementBy = 1;

        public bool appendBranch = false;

        public bool printBuildVersion = true;

#if ODIN_INSPECTOR || TRI_INSPECTOR
        [ShowIf(nameof(printBuildVersion))]
#endif
        public string versionLocation = "Builds/version.txt";
        
        public override void Execute(IUniBuilderConfiguration configuration)
        {
            var buildParameters = configuration.BuildParameters;
            var branch = appendBranch 
                ? configuration.BuildParameters.branch 
                : string.Empty;
            
            var buildNumber = buildParameters.buildNumber == 0 
                ? BuildVersionProvider.GetActiveBuildNumber(buildParameters.buildTarget) 
                : buildParameters.buildNumber;
            
            var buildTarget = buildParameters.buildTarget == BuildTarget.NoTarget 
                ? EditorUserBuildSettings.activeBuildTarget 
                : buildParameters.buildTarget;
            
            UpdateBuildVersion(buildTarget, buildNumber, branch);
        }

#if ODIN_INSPECTOR || TRI_INSPECTOR
        [Button]
#endif
        public void Execute()
        {
            var branch = appendBranch ?  GitCommands.GetGitBranch() : string.Empty;
            var buildNumber = BuildVersionProvider.GetActiveBuildNumber(EditorUserBuildSettings.activeBuildTarget);
            UpdateBuildVersion(EditorUserBuildSettings.activeBuildTarget,buildNumber , branch);
            
            if(printBuildVersion) 
                PrintBuildVersion();
        }

        public void PrintBuildVersion()
        {
            try
            {
                var targetPath = FileUtils.ProjectPath.CombinePath(versionLocation);
                Debug.Log("version saved to " + targetPath);
                File.WriteAllText(targetPath,PlayerSettings.bundleVersion);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
        }
        
        public void UpdateBuildVersion(BuildTarget buildTarget,int buildNumber, string branch) 
        {
            var logBuilder = new StringBuilder(200);

            var activeBuildNumber  = Mathf.Max(buildNumber, minBuildNumber);
            activeBuildNumber = Mathf.Max(1, activeBuildNumber);
            var resultBuildNumber  = activeBuildNumber + incrementBy;
            
            var bundleVersion     = BuildVersionProvider
                .GetBuildVersion(buildTarget, PlayerSettings.bundleVersion, resultBuildNumber, branch);
            
            PlayerSettings.bundleVersion = bundleVersion;
            var buildNumberString =  resultBuildNumber.ToString();
            PlayerSettings.iOS.buildNumber = buildNumberString;
            PlayerSettings.Android.bundleVersionCode = resultBuildNumber;
            
            logBuilder.Append("\tUNIBUILD: Parameters build number : ");
            logBuilder.Append(buildNumber);
            logBuilder.AppendLine();
 
            logBuilder.Append("\tUNIBUILD: ResultBuildNumber build number : ");
            logBuilder.Append(resultBuildNumber);
            logBuilder.AppendLine();
                                  
            logBuilder.Append("\tUNIBUILD: PlayerSettings.bundleVersion : ");
            logBuilder.Append(bundleVersion);
            logBuilder.AppendLine();
            
            logBuilder.Append("\tUNIBUILD: PlayerSettings.iOS.buildNumber : ");
            logBuilder.Append(buildNumberString);
            logBuilder.AppendLine();
            
            logBuilder.Append("\tUNIBUILD: PlayerSettings.Android.bundleVersionCode : ");
            logBuilder.Append(resultBuildNumber);
            logBuilder.AppendLine();
            
            Debug.Log(logBuilder);
        }
    }
    
}
