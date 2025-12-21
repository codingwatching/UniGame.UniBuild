namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Google.Apis.Sheets.v4.Data;
    using UniModules.UniGame.UniBuild;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using CustomTabView = UniGame.UniBuild.Editor.Inspector.Views.TabView;
    using Color = UnityEngine.Color;

    /// <summary>
    /// Main editor window for managing Build Pipelines
    /// Features:
    /// - View all available pipelines
    /// - Run pipeline execution
    /// - Edit pipeline (add/remove steps)
    /// - View command catalog with descriptions
    /// - Create new pipelines with configurable path
    /// - Run individual steps
    /// - Search pipelines and steps
    /// - Settings management
    /// </summary>
    public class BuildPipelineEditorWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "Build Pipeline Editor";
        private const string UXML_PATH = "BuildPipelineEditor";
        private const string USS_PATH = "BuildPipelineEditor";

        // UI Elements
        private VisualElement _root;
        private CustomTabView _mainTabs;
        private VisualElement _pipelineListContainer;
        private VisualElement _pipelineEditorContainer;
        private VisualElement _commandCatalogContainer;
        private VisualElement _settingsContainer;
        private TextField _pipelineSearchField;
        private TextField _commandSearchField;
        private TextField _stepSearchField;
        private ScrollView _pipelineScrollView;
        private ScrollView _commandScrollView;
        private Button _createPipelineButton;
        private Button _executePipelineButton;
        private Button _refreshButton;
        private Label _statusLabel;

        // Data
        private BuildPipelineInspectorSettings _settings;
        private List<ScriptableCommandsGroup> _loadedPipelines = new List<ScriptableCommandsGroup>();
        private ScriptableCommandsGroup _selectedPipeline;
        private Dictionary<Type, BuildCommandMetadataAttribute> _commandMetadata;
        private List<PipelineExecutionState> _executionHistory = new List<PipelineExecutionState>();

        [MenuItem("UniGame/Uni Build/Pipeline Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildPipelineEditorWindow>();
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.minSize = new Vector2(1000, 600);
        }

        private void OnEnable()
        {
            _settings = BuildPipelineSettingsManager.GetSettings();
            CollectCommandMetadata();
        }

        private void CreateGUI()
        {
            _root = rootVisualElement;

            // Load USS Stylesheet
            LoadStylesheet();

            // Create UI Structure
            CreateMainUI();

            // Register event handlers
            RegisterEventHandlers();
        }

        private void LoadStylesheet()
        {
            // Find the Inspector folder to load styles relative to it
            var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var scriptFolder = Path.GetDirectoryName(scriptPath); // Editor/Inspector/Editors
            var inspectorFolder = Path.GetDirectoryName(scriptFolder); // Editor/Inspector
            var ussPath = Path.Combine(inspectorFolder, "Styles", $"{USS_PATH}.uss");
            var relativeUssPath = ussPath.Replace(Application.dataPath, "Assets");

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(relativeUssPath);
            if (styleSheet != null)
            {
                _root.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning($"Failed to load stylesheet at: {relativeUssPath}");
            }
        }

        private void CreateMainUI()
        {
            _root.Clear();

            // Header
            var header = CreateHeader();
            _root.Add(header);

            // Main tab view
            _mainTabs = new CustomTabView();
            CreatePipelineListTab();
            CreatePipelineEditorTab();
            CreateCommandCatalogTab();
            CreateSettingsTab();

            _root.Add(_mainTabs);

            // Load pipelines after UI is created
            LoadPipelines();
        }

        private VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.AddToClassList("header");

            var title = new Label(WINDOW_TITLE);
            title.AddToClassList("header-title");
            header.Add(title);

            var controls = new VisualElement();
            controls.AddToClassList("header-controls");

            _refreshButton = new Button(RefreshPipelines) { text = "Refresh" };
            _refreshButton.AddToClassList("button-control");
            controls.Add(_refreshButton);

            _statusLabel = new Label("Ready");
            _statusLabel.AddToClassList("status-label");
            controls.Add(_statusLabel);

            header.Add(controls);

            return header;
        }

        private void CreatePipelineListTab()
        {
            var tab = new CustomTabView.Tab() { label = "Pipelines", content = new VisualElement() };
            tab.content.style.flexDirection = FlexDirection.Column;
            tab.content.style.flexGrow = 1;

            // Search and create controls
            var topControls = new VisualElement();
            topControls.style.flexDirection = FlexDirection.Row;
            topControls.style.paddingLeft = 8;
            topControls.style.paddingRight = 8;
            topControls.style.paddingTop = 8;
            topControls.style.paddingBottom = 8;

            _pipelineSearchField = new TextField();
            _pipelineSearchField.value = "";
            _pipelineSearchField.style.flexGrow = 1;
            _pipelineSearchField.RegisterValueChangedCallback(evt => FilterPipelines(evt.newValue));
            topControls.Add(_pipelineSearchField);

            _createPipelineButton = new Button(ShowCreatePipelineDialog) { text = "Create Pipeline" };
            _createPipelineButton.AddToClassList("button-create");
            topControls.Add(_createPipelineButton);

            tab.content.Add(topControls);

            // Pipeline list scroll view
            _pipelineScrollView = new ScrollView();
            _pipelineScrollView.style.flexGrow = 1;
            _pipelineListContainer = new VisualElement();
            _pipelineListContainer.style.flexDirection = FlexDirection.Column;
            _pipelineScrollView.Add(_pipelineListContainer);
            tab.content.Add(_pipelineScrollView);

            _mainTabs.Add(tab);
        }

        private void CreatePipelineEditorTab()
        {
            var tab = new CustomTabView.Tab() { label = "Editor", content = new VisualElement() };
            tab.content.style.flexDirection = FlexDirection.Column;
            tab.content.style.flexGrow = 1;

            // Editor header
            var editorHeader = new VisualElement();
            editorHeader.style.flexDirection = FlexDirection.Row;
            editorHeader.style.paddingLeft = 8;
            editorHeader.style.paddingRight = 8;
            editorHeader.style.paddingTop = 8;
            editorHeader.style.paddingBottom = 8;

            var selectedLabel = new Label("Selected: ");
            editorHeader.Add(selectedLabel);

            var selectedNameLabel = new Label("None");
            selectedNameLabel.style.flexGrow = 1;
            selectedNameLabel.AddToClassList("selected-pipeline-name");
            editorHeader.Add(selectedNameLabel);

            _executePipelineButton = new Button(ExecuteSelectedPipeline) { text = "Execute" };
            _executePipelineButton.AddToClassList("button-execute");
            editorHeader.Add(_executePipelineButton);

            tab.content.Add(editorHeader);

            // Step search
            _stepSearchField = new TextField();
            _stepSearchField.value = "";
            _stepSearchField.style.marginLeft = 8;
            _stepSearchField.style.marginRight = 8;
            _stepSearchField.style.marginTop = 8;
            _stepSearchField.style.marginBottom = 8;
            _stepSearchField.RegisterValueChangedCallback(evt => FilterPipelineSteps(evt.newValue));
            tab.content.Add(_stepSearchField);

            // Editor container
            _pipelineEditorContainer = new ScrollView();
            _pipelineEditorContainer.style.flexGrow = 1;
            tab.content.Add(_pipelineEditorContainer);

            _mainTabs.Add(tab);
        }

        private void CreateCommandCatalogTab()
        {
            var tab = new CustomTabView.Tab() { label = "Command Catalog", content = new VisualElement() };
            tab.content.style.flexDirection = FlexDirection.Column;
            tab.content.style.flexGrow = 1;

            // Search
            _commandSearchField = new TextField();
            _commandSearchField.value = "";
            _commandSearchField.style.marginLeft = 8;
            _commandSearchField.style.marginRight = 8;
            _commandSearchField.style.marginTop = 8;
            _commandSearchField.style.marginBottom = 8;
            _commandSearchField.RegisterValueChangedCallback(evt => FilterCommandCatalog(evt.newValue));
            tab.content.Add(_commandSearchField);

            // Command list
            _commandScrollView = new ScrollView();
            _commandScrollView.style.flexGrow = 1;
            _commandCatalogContainer = new VisualElement();
            _commandCatalogContainer.style.flexDirection = FlexDirection.Column;
            _commandScrollView.Add(_commandCatalogContainer);
            tab.content.Add(_commandScrollView);

            _mainTabs.Add(tab);

            // Populate command catalog
            PopulateCommandCatalog();
        }

        private void CreateSettingsTab()
        {
            var tab = new CustomTabView.Tab() { label = "Settings", content = new VisualElement() };
            tab.content.style.flexDirection = FlexDirection.Column;
            tab.content.style.paddingLeft = 16;
            tab.content.style.paddingRight = 16;
            tab.content.style.paddingTop = 16;
            tab.content.style.paddingBottom = 16;

            _settingsContainer = new VisualElement();
            _settingsContainer.style.flexDirection = FlexDirection.Column;

            // Path settings
            var pathField = new TextField("Pipeline Creation Path");
            pathField.value = _settings.PipelineCreationPath;
            pathField.RegisterValueChangedCallback(evt =>
            {
                _settings.PipelineCreationPath = evt.newValue;
            });
            _settingsContainer.Add(pathField);

            // Auto refresh toggle
            var autoRefreshToggle = new Toggle("Auto Refresh Pipelines");
            autoRefreshToggle.value = _settings.AutoRefreshPipelines;
            autoRefreshToggle.RegisterValueChangedCallback(evt =>
            {
                _settings.AutoRefreshPipelines = evt.newValue;
            });
            _settingsContainer.Add(autoRefreshToggle);

            // History size
            var historySizeField = new IntegerField("Max History Size");
            historySizeField.value = _settings.MaxHistorySize;
            historySizeField.RegisterValueChangedCallback(evt =>
            {
                _settings.MaxHistorySize = evt.newValue;
            });
            _settingsContainer.Add(historySizeField);

            // Show descriptions toggle
            var showDescriptionsToggle = new Toggle("Show Command Descriptions");
            showDescriptionsToggle.value = _settings.ShowCommandDescriptions;
            showDescriptionsToggle.RegisterValueChangedCallback(evt =>
            {
                _settings.ShowCommandDescriptions = evt.newValue;
            });
            _settingsContainer.Add(showDescriptionsToggle);

            // Detailed logging toggle
            var detailedLoggingToggle = new Toggle("Enable Detailed Logging");
            detailedLoggingToggle.value = _settings.EnableDetailedLogging;
            detailedLoggingToggle.RegisterValueChangedCallback(evt =>
            {
                _settings.EnableDetailedLogging = evt.newValue;
            });
            _settingsContainer.Add(detailedLoggingToggle);

            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.Add(_settingsContainer);
            tab.content.Add(scrollView);

            _mainTabs.Add(tab);
        }

        private void CollectCommandMetadata()
        {
            _commandMetadata = new Dictionary<Type, BuildCommandMetadataAttribute>();

            var commandTypes = TypeCache.GetTypesDerivedFrom<SerializableBuildCommand>();
            foreach (var type in commandTypes)
            {
                if (type.IsAbstract) continue;

                var metadata = type.GetCustomAttribute<BuildCommandMetadataAttribute>();
                if (metadata != null)
                {
                    _commandMetadata[type] = metadata;
                }
            }
        }

        private void PopulateCommandCatalog()
        {
            _commandCatalogContainer.Clear();

            var groups = _commandMetadata
                .GroupBy(x => x.Value.Category)
                .OrderBy(x => x.Key);

            foreach (var group in groups)
            {
                var groupFoldout = new Foldout { text = group.Key, value = true };
                groupFoldout.AddToClassList("command-category");

                foreach (var commandEntry in group.OrderBy(x => x.Value.DisplayName))
                {
                    var commandItem = CreateCommandCatalogItem(commandEntry.Key, commandEntry.Value);
                    groupFoldout.Add(commandItem);
                }

                _commandCatalogContainer.Add(groupFoldout);
            }
        }

        private VisualElement CreateCommandCatalogItem(Type commandType, BuildCommandMetadataAttribute metadata)
        {
            var item = new VisualElement();
            item.AddToClassList("command-catalog-item");
            item.style.paddingLeft = 16;
            item.style.paddingTop = 8;
            item.style.paddingBottom = 8;
            item.style.borderBottomWidth = 1;
            item.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));

            var nameLabel = new Label(metadata.DisplayName ?? commandType.Name);
            nameLabel.AddToClassList("command-name");
            item.Add(nameLabel);

            if (!string.IsNullOrEmpty(metadata.Description))
            {
                var descLabel = new Label(metadata.Description);
                descLabel.AddToClassList("command-description");
                item.Add(descLabel);
            }

            var typeLabel = new Label($"Type: {commandType.FullName}");
            typeLabel.style.fontSize = 10;
            typeLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            item.Add(typeLabel);

            return item;
        }

        private void LoadPipelines()
        {
            _loadedPipelines.Clear();

            var guids = AssetDatabase.FindAssets("t:ScriptableCommandsGroup");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var pipeline = AssetDatabase.LoadAssetAtPath<ScriptableCommandsGroup>(path);
                if (pipeline != null)
                {
                    _loadedPipelines.Add(pipeline);
                }
            }

            RefreshPipelineList();
        }

        private void RefreshPipelines()
        {
            LoadPipelines();
            UpdateStatusLabel("Pipelines refreshed");
        }

        private void RefreshPipelineList()
        {
            _pipelineListContainer.Clear();

            foreach (var pipeline in _loadedPipelines.OrderBy(p => p.name))
            {
                var pipelineItem = CreatePipelineListItem(pipeline);
                _pipelineListContainer.Add(pipelineItem);
            }
        }

        private VisualElement CreatePipelineListItem(ScriptableCommandsGroup pipeline)
        {
            var item = new Button(() => SelectPipeline(pipeline));
            item.AddToClassList("pipeline-list-item");
            item.style.height = 60;
            item.style.justifyContent = Justify.FlexStart;
            item.style.alignItems = Align.FlexStart;
            item.style.paddingLeft = 8;
            item.style.paddingRight = 8;
            item.style.paddingTop = 8;
            item.style.paddingBottom = 8;
            item.style.marginBottom = 4;
            item.style.flexDirection = FlexDirection.Column;

            var name = new Label(pipeline.name);
            name.AddToClassList("pipeline-name");
            item.Add(name);

            var info = new Label($"Steps: {pipeline.commands.commands.Count}");
            info.style.fontSize = 11;
            info.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            item.Add(info);

            return item;
        }

        private void SelectPipeline(ScriptableCommandsGroup pipeline)
        {
            _selectedPipeline = pipeline;
            RefreshPipelineEditor();
            UpdateStatusLabel($"Selected: {pipeline.name}");
        }

        private void RefreshPipelineEditor()
        {
            _pipelineEditorContainer.Clear();

            if (_selectedPipeline == null)
            {
                _pipelineEditorContainer.Add(new Label("No pipeline selected"));
                return;
            }

            // Pipeline info
            var infoBox = new VisualElement();
            infoBox.style.paddingLeft = 8;
            infoBox.style.paddingRight = 8;
            infoBox.style.paddingTop = 8;
            infoBox.style.paddingBottom = 8;
            infoBox.style.marginBottom = 8;
            infoBox.style.borderTopWidth = 1;
            infoBox.style.borderRightWidth = 1;
            infoBox.style.borderBottomWidth = 1;
            infoBox.style.borderLeftWidth = 1;
            var borderColor = new Color(0.3f, 0.3f, 0.3f);
            infoBox.style.borderTopColor = new StyleColor(borderColor);
            infoBox.style.borderRightColor = new StyleColor(borderColor);
            infoBox.style.borderBottomColor = new StyleColor(borderColor);
            infoBox.style.borderLeftColor = new StyleColor(borderColor);

            infoBox.Add(new Label($"Pipeline: {_selectedPipeline.name}"));
            infoBox.Add(new Label($"Total Steps: {_selectedPipeline.commands.commands.Count}"));

            _pipelineEditorContainer.Add(infoBox);

            // Steps list
            if (_selectedPipeline.commands.commands.Count == 0)
            {
                _pipelineEditorContainer.Add(new Label("No steps added"));
                return;
            }

            foreach (var command in _selectedPipeline.commands.commands)
            {
                var stepItem = CreatePipelineStepItem(command);
                _pipelineEditorContainer.Add(stepItem);
            }
        }

        private VisualElement CreatePipelineStepItem(BuildCommandStep step)
        {
            var item = new VisualElement();
            item.AddToClassList("pipeline-step-item");
            item.style.flexDirection = FlexDirection.Row;
            item.style.paddingLeft = 8;
            item.style.paddingRight = 8;
            item.style.paddingTop = 8;
            item.style.paddingBottom = 8;
            item.style.marginBottom = 4;
            item.style.borderTopWidth = 1;
            item.style.borderRightWidth = 1;
            item.style.borderBottomWidth = 1;
            item.style.borderLeftWidth = 1;
            
            var borderColor = new Color(0.3f, 0.3f, 0.3f);
            item.style.borderTopColor = new StyleColor(borderColor);
            item.style.borderRightColor = new StyleColor(borderColor);
            item.style.borderBottomColor = new StyleColor(borderColor);
            item.style.borderLeftColor = new StyleColor(borderColor);

            // Get the first command from the step for display
            var commands = step.GetCommands().ToList();
            if (commands.Count == 0)
                return item;

            var command = commands[0];

            // Active toggle
            var activeToggle = new Toggle();
            activeToggle.value = command.IsActive;
            activeToggle.RegisterValueChangedCallback(evt =>
            {
                var field = command.GetType().GetField("isActive", BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(command, evt.newValue);
                    EditorUtility.SetDirty(_selectedPipeline);
                }
            });
            item.Add(activeToggle);

            // Command name
            var nameLabel = new Label(command.Name);
            nameLabel.style.flexGrow = 1;
            item.Add(nameLabel);

            // Execute button
            var executeButton = new Button(() => ExecuteStep(command)) { text = "Run" };
            executeButton.style.width = 50;
            item.Add(executeButton);

            // Delete button
            var deleteButton = new Button(() => RemoveStep(step)) { text = "Remove" };
            deleteButton.style.width = 70;
            deleteButton.AddToClassList("button-danger");
            item.Add(deleteButton);

            return item;
        }

        private void ExecuteStep(IUnityBuildCommand command)
        {
            try
            {
                UpdateStatusLabel($"Executing: {command.Name}...");

                var config = new EditorBuildConfiguration(null, null);
                command.Execute(config);

                UpdateStatusLabel($"✓ Executed: {command.Name}");
            }
            catch (Exception ex)
            {
                UpdateStatusLabel($"✗ Error: {ex.Message}");
                Debug.LogError($"Failed to execute step: {ex}", _selectedPipeline);
            }
        }

        private void RemoveStep(BuildCommandStep step)
        {
            if (_selectedPipeline == null) return;

            _selectedPipeline.commands.commands.Remove(step);
            EditorUtility.SetDirty(_selectedPipeline);
            RefreshPipelineEditor();
            UpdateStatusLabel("Step removed");
        }

        private void ExecuteSelectedPipeline()
        {
            if (_selectedPipeline == null)
            {
                UpdateStatusLabel("No pipeline selected");
                return;
            }

            try
            {
                UpdateStatusLabel($"Executing pipeline: {_selectedPipeline.name}...");

                var startTime = EditorApplication.timeSinceStartup;
                var executionState = new PipelineExecutionState(_selectedPipeline.name);

                var config = new EditorBuildConfiguration(null, null);

                foreach (var step in _selectedPipeline.commands.commands)
                {
                    foreach (var command in step.GetCommands())
                    {
                        if (!command.IsActive) continue;

                        try
                        {
                            command.Execute(config);
                            executionState.AddStepExecution(command.Name, true);
                        }
                        catch (Exception ex)
                        {
                            executionState.AddStepExecution(command.Name, false, ex.Message);
                            throw;
                        }
                    }
                }

                var executionTime = (float)(EditorApplication.timeSinceStartup - startTime);
                executionState.SetExecutionTime(executionTime);
                executionState.SetResult(true);

                _executionHistory.Add(executionState);
                if (_executionHistory.Count > _settings.MaxHistorySize)
                {
                    _executionHistory.RemoveAt(0);
                }

                UpdateStatusLabel($"✓ Pipeline executed successfully ({executionTime:F2}s)");
            }
            catch (Exception ex)
            {
                UpdateStatusLabel($"✗ Pipeline execution failed: {ex.Message}");
                Debug.LogError($"Pipeline execution error: {ex}", _selectedPipeline);
            }
        }

        private void ShowCreatePipelineDialog()
        {
            var dialog = EditorUtility.DisplayDialog(
                "Create New Pipeline",
                "Enter the name for the new pipeline:",
                "Create",
                "Cancel"
            );

            if (dialog)
            {
                var path = _settings.PipelineCreationPath;
                if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path))
                {
                    path = "Assets/BuildPipelines";
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        AssetDatabase.CreateFolder("Assets", "BuildPipelines");
                    }
                }

                var pipeline = ScriptableObject.CreateInstance<ScriptableCommandsGroup>();
                var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/BuildPipeline.asset");
                AssetDatabase.CreateAsset(pipeline, assetPath);
                AssetDatabase.SaveAssets();

                LoadPipelines();
                UpdateStatusLabel("Pipeline created");
            }
        }

        private void FilterPipelines(string filter)
        {
            _pipelineListContainer.Clear();

            var filtered = _loadedPipelines
                .Where(p => p.name.Contains(filter, System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.name);

            foreach (var pipeline in filtered)
            {
                var item = CreatePipelineListItem(pipeline);
                _pipelineListContainer.Add(item);
            }
        }

        private void FilterPipelineSteps(string filter)
        {
            if (_selectedPipeline == null) return;
            RefreshPipelineEditor();
            // Could further filter the displayed steps
        }

        private void FilterCommandCatalog(string filter)
        {
            _commandCatalogContainer.Clear();

            var filtered = _commandMetadata
                .Where(x => x.Value.DisplayName.Contains(filter, System.StringComparison.OrdinalIgnoreCase) ||
                            x.Value.Description.Contains(filter, System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Value.Category);

            var groups = filtered.GroupBy(x => x.Value.Category);

            foreach (var group in groups)
            {
                var groupFoldout = new Foldout { text = group.Key, value = true };

                foreach (var commandEntry in group.OrderBy(x => x.Value.DisplayName))
                {
                    var commandItem = CreateCommandCatalogItem(commandEntry.Key, commandEntry.Value);
                    groupFoldout.Add(commandItem);
                }

                _commandCatalogContainer.Add(groupFoldout);
            }
        }

        private void RegisterEventHandlers()
        {
            // Can be extended for additional event handling
        }

        private void UpdateStatusLabel(string message)
        {
            _statusLabel.text = $"{message} [{System.DateTime.Now:HH:mm:ss}]";
        }

        private void OnDestroy()
        {
            // Save settings when window is closed
            if (_settings != null)
            {
                BuildPipelineSettingsManager.SaveSettings(_settings);
            }
        }
    }
}
