namespace UniGame.UniBuild.Editor.Inspector
{
    using UnityEditor;
    using Editors;

    /// <summary>
    /// Menu items for Build Pipeline Editor
    /// Provides convenient access points from the main menu
    /// </summary>
    public static class BuildPipelineMenu
    {
        [MenuItem("UniGame/Build Pipeline/Pipeline Editor", priority = 100)]
        public static void OpenPipelineEditor()
        {
            BuildPipelineEditorWindow.ShowWindow();
        }

        [MenuItem("UniGame/Build Pipeline/Create Pipeline...", priority = 20)]
        public static void CreateNewPipeline()
        {
            var folderPath = "Assets/BuildPipelines";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "BuildPipelines");
                AssetDatabase.Refresh();
            }

            var pipeline = PipelineManager.CreatePipeline(folderPath, "NewPipeline");
            EditorGUIUtility.PingObject(pipeline);
            Selection.activeObject = pipeline;
        }

        [MenuItem("UniGame/Build Pipeline/Find Pipelines", priority = 30)]
        public static void FindAllPipelines()
        {
            var pipelines = PipelineManager.LoadAllPipelines();
            if (pipelines.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Pipelines Found",
                    "No build pipelines found in the project.",
                    "OK"
                );
                return;
            }

            // Display results in console
            UnityEngine.Debug.Log($"Found {pipelines.Count} pipeline(s):");
            foreach (var pipeline in pipelines)
            {
                var (totalCount, activeCount) = PipelineManager.GetPipelineStats(pipeline);
                var path = PipelineManager.GetPipelinePath(pipeline);
                UnityEngine.Debug.Log(
                    $"  - {pipeline.name} ({activeCount}/{totalCount} active steps) - {path}",
                    pipeline
                );
            }
        }

        [MenuItem("UniGame/Build Pipeline/Command Catalog", priority = 40)]
        public static void ShowCommandCatalog()
        {
            var window = EditorWindow.GetWindow<BuildPipelineEditorWindow>();
            window.titleContent = new UnityEngine.GUIContent("Build Pipeline Editor");
            window.minSize = new UnityEngine.Vector2(1000, 600);
            // The window will show the Command Catalog tab
        }
    }
}
