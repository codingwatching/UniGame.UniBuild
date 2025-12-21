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
        private List<UniBuildPipeline> _loadedPipelines = new List<UniBuildPipeline>();
        private UniBuildPipeline _selectedPipeline;
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
            _settings = BuildPipelineInspectorSettings.GetSettings();
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

            // Main container with tabs
            _mainTabs = new CustomTabView();

            // First tab: Pipelines with split view (list + editor)
            var pipelinesTab = new CustomTabView.Tab() { label = "Pipelines", content = new VisualElement() };
            pipelinesTab.content.style.flexDirection = FlexDirection.Row;
            pipelinesTab.content.style.flexGrow = 1;

            // Left panel: Pipeline list
            var leftPanel = CreatePipelineListPanel();
            pipelinesTab.content.Add(leftPanel);

            // Right panel: Pipeline editor
            var rightPanel = CreatePipelineEditorPanel();
            pipelinesTab.content.Add(rightPanel);

            _mainTabs.Add(pipelinesTab);

            // Other tabs
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

        private VisualElement CreatePipelineListPanel()
        {
            var panel = new VisualElement();
            panel.style.flexDirection = FlexDirection.Column;
            panel.style.width = 300;
            panel.style.borderRightWidth = 1;
            panel.style.borderRightColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

            // Search and create controls
            var topControls = new VisualElement();
            topControls.style.flexDirection = FlexDirection.Column;
            topControls.style.paddingLeft = 8;
            topControls.style.paddingRight = 8;
            topControls.style.paddingTop = 8;
            topControls.style.paddingBottom = 8;
            topControls.style.borderBottomWidth = 1;
            topControls.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

            _pipelineSearchField = new TextField();
            _pipelineSearchField.value = "";
            _pipelineSearchField.style.marginBottom = 4;
            _pipelineSearchField.RegisterValueChangedCallback(evt => FilterPipelines(evt.newValue));
            topControls.Add(_pipelineSearchField);

            _createPipelineButton = new Button(ShowCreatePipelineDialog) { text = "Create Pipeline" };
            _createPipelineButton.AddToClassList("button-create");
            topControls.Add(_createPipelineButton);

            panel.Add(topControls);

            // Pipeline list scroll view
            _pipelineScrollView = new ScrollView();
            _pipelineScrollView.style.flexGrow = 1;
            _pipelineListContainer = new VisualElement();
            _pipelineListContainer.style.flexDirection = FlexDirection.Column;
            _pipelineScrollView.Add(_pipelineListContainer);
            panel.Add(_pipelineScrollView);

            return panel;
        }

        private VisualElement CreatePipelineEditorPanel()
        {
            var panel = new VisualElement();
            panel.style.flexDirection = FlexDirection.Column;
            panel.style.flexGrow = 1;

            // Editor header
            var editorHeader = new VisualElement();
            editorHeader.style.flexDirection = FlexDirection.Row;
            editorHeader.style.paddingLeft = 8;
            editorHeader.style.paddingRight = 8;
            editorHeader.style.paddingTop = 8;
            editorHeader.style.paddingBottom = 8;
            editorHeader.style.borderBottomWidth = 1;
            editorHeader.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

            var selectedLabel = new Label("Selected: ");
            editorHeader.Add(selectedLabel);

            var selectedNameLabel = new Label("None");
            selectedNameLabel.style.flexGrow = 1;
            selectedNameLabel.AddToClassList("selected-pipeline-name");
            editorHeader.Add(selectedNameLabel);

            _executePipelineButton = new Button(ExecuteSelectedPipeline) { text = "Execute" };
            _executePipelineButton.AddToClassList("button-execute");
            editorHeader.Add(_executePipelineButton);

            var pingButton = new Button(PingSelectedPipeline) { text = "Ping" };
            pingButton.style.width = 60;
            editorHeader.Add(pingButton);

            panel.Add(editorHeader);

            // Step search
            _stepSearchField = new TextField();
            _stepSearchField.value = "";
            _stepSearchField.style.marginLeft = 8;
            _stepSearchField.style.marginRight = 8;
            _stepSearchField.style.marginTop = 8;
            _stepSearchField.style.marginBottom = 8;
            _stepSearchField.RegisterValueChangedCallback(evt => FilterPipelineSteps(evt.newValue));
            panel.Add(_stepSearchField);

            // Editor container
            _pipelineEditorContainer = new ScrollView();
            _pipelineEditorContainer.style.flexGrow = 1;
            panel.Add(_pipelineEditorContainer);

            return panel;
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

            var guids = AssetDatabase.FindAssets("t:UniBuildPipeline");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var pipeline = AssetDatabase.LoadAssetAtPath<UniBuildPipeline>(path);
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

        private VisualElement CreatePipelineListItem(UniBuildPipeline pipeline)
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

            // Get asset file name from the asset path
            var assetPath = AssetDatabase.GetAssetPath(pipeline);
            var assetName = Path.GetFileNameWithoutExtension(assetPath);
            if (string.IsNullOrEmpty(assetName))
                assetName = pipeline.name;

            var name = new Label(assetName);
            name.AddToClassList("pipeline-name");
            item.Add(name);

            var info = new Label($"Steps: {pipeline.preBuildCommands.Count + pipeline.postBuildCommands.Count}");
            info.style.fontSize = 11;
            info.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            item.Add(info);

            return item;
        }

        private void SelectPipeline(UniBuildPipeline pipeline)
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
            infoBox.Add(new Label($"Total Steps: {_selectedPipeline.preBuildCommands.Count + _selectedPipeline.postBuildCommands.Count}"));

            _pipelineEditorContainer.Add(infoBox);

            // Steps list
            if (_selectedPipeline.preBuildCommands.Count == 0 && _selectedPipeline.postBuildCommands.Count == 0)
            {
                _pipelineEditorContainer.Add(new Label("No steps added"));
                return;
            }

            // Pre-build commands
            foreach (var command in _selectedPipeline.preBuildCommands)
            {
                var stepItem = CreatePipelineStepItem(command);
                _pipelineEditorContainer.Add(stepItem);
            }

            // Post-build commands
            foreach (var command in _selectedPipeline.postBuildCommands)
            {
                var stepItem = CreatePipelineStepItem(command);
                _pipelineEditorContainer.Add(stepItem);
            }
        }

        private VisualElement CreatePipelineStepItem(BuildCommandStep step)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.marginBottom = 12;
            container.style.borderBottomWidth = 1;
            container.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            container.style.paddingBottom = 12;

            // Get all commands from the step
            var commands = step.GetCommands().ToList();

            // Determine step title based on command
            string stepTitle;
            if (commands.Count == 0)
            {
                stepTitle = "Step (no commands)";
            }
            else
            {
                var firstCommand = commands[0];
                if (firstCommand is PipelineCommandsGroup group)
                {
                    // For groups, show the group name + nested command count
                    var nestedCount = group.commands.Commands.Count();
                    stepTitle = $"{firstCommand.Name} ({nestedCount} command{(nestedCount != 1 ? "s" : "")})";
                }
                else
                {
                    // For regular commands, show the command name
                    stepTitle = firstCommand.Name;
                }
            }

            // Create foldout for the entire step
            var stepFoldout = new Foldout
            {
                text = stepTitle,
                value = true
            };
            stepFoldout.style.fontSize = 12;
            stepFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            stepFoldout.style.paddingLeft = 0;
            stepFoldout.style.marginBottom = 0;

            // Step header with Run and Remove buttons on the same line
            var stepHeaderRow = new VisualElement();
            stepHeaderRow.style.flexDirection = FlexDirection.Row;
            stepHeaderRow.style.alignItems = Align.Center;
            stepHeaderRow.style.justifyContent = Justify.FlexEnd;
            stepHeaderRow.style.paddingLeft = 8;
            stepHeaderRow.style.paddingRight = 8;
            stepHeaderRow.style.paddingTop = 4;
            stepHeaderRow.style.paddingBottom = 4;
            stepHeaderRow.style.marginBottom = 0;

            // Run button for the step
            var runStepButton = new Button(() => ExecuteStep(step.GetCommands().FirstOrDefault())) { text = "Run" };
            runStepButton.style.width = 50;
            runStepButton.style.marginRight = 4;
            stepHeaderRow.Add(runStepButton);

            // Remove button for the step
            var deleteButton = new Button(() => RemoveStep(step)) { text = "Remove Step" };
            deleteButton.style.width = 100;
            deleteButton.AddToClassList("button-danger");
            stepHeaderRow.Add(deleteButton);

            // Add header to foldout's toggle area
            stepFoldout.Add(stepHeaderRow);

            // Create content container for all commands
            var contentContainer = new VisualElement();
            contentContainer.style.flexDirection = FlexDirection.Column;
            contentContainer.style.paddingLeft = 12;

            // Check if step has only one regular command (not a group)
            bool isSingleRegularCommand = commands.Count == 1 && !(commands[0] is PipelineCommandsGroup);

            // Display each command with its own inspector
            if (commands.Count == 0)
            {
                var emptyLabel = new Label("No commands in this step");
                emptyLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                emptyLabel.style.fontSize = 10;
                emptyLabel.style.marginLeft = 16;
                emptyLabel.style.marginTop = 8;
                contentContainer.Add(emptyLabel);
            }
            else if (isSingleRegularCommand)
            {
                // For single regular command, display properties directly without wrapper
                var command = commands[0];
                
                // Command properties directly in content (no wrapper, no duplicate buttons)
                var inspectorContainer = new VisualElement();
                inspectorContainer.style.flexDirection = FlexDirection.Column;
                inspectorContainer.style.paddingLeft = 4;
                inspectorContainer.style.paddingTop = 4;
                inspectorContainer.style.paddingRight = 4;
                inspectorContainer.style.marginLeft = 0;
                inspectorContainer.style.marginRight = 0;
                
                // Add toggle and type info as header
                var commandHeaderRow = new VisualElement();
                commandHeaderRow.style.flexDirection = FlexDirection.Row;
                commandHeaderRow.style.alignItems = Align.Center;
                commandHeaderRow.style.marginBottom = 8;
                commandHeaderRow.style.paddingLeft = 4;
                commandHeaderRow.style.paddingRight = 4;
                
                var toggle = new Toggle();
                toggle.value = command.IsActive;
                toggle.style.marginRight = 6;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    var field = command.GetType().GetField("isActive", BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    }
                });
                commandHeaderRow.Add(toggle);
                
                var typeLabel = new Label(command.GetType().Name);
                typeLabel.style.fontSize = 9;
                typeLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                commandHeaderRow.Add(typeLabel);
                
                inspectorContainer.Add(commandHeaderRow);
                DisplayCommandProperties(command, inspectorContainer);
                
                contentContainer.Add(inspectorContainer);
            }
            else
            {
                // Display each command
                for (int i = 0; i < commands.Count; i++)
                {
                    var command = commands[i];
                    if (command == null) continue;

                    // Check if this is a PipelineCommandsGroup
                    if (command is PipelineCommandsGroup group)
                    {
                        // For group commands, show each nested command as separate item
                        var nestedCommandsList = group.commands.Commands.ToList();
                        
                        var groupHeaderContainer = new VisualElement();
                        groupHeaderContainer.style.flexDirection = FlexDirection.Column;
                        groupHeaderContainer.style.marginLeft = 8;
                        groupHeaderContainer.style.marginRight = 8;
                        groupHeaderContainer.style.marginTop = 8;
                        groupHeaderContainer.style.marginBottom = 8;

                        // Group header
                        var groupHeaderRow = new VisualElement();
                        groupHeaderRow.style.flexDirection = FlexDirection.Row;
                        groupHeaderRow.style.alignItems = Align.Center;
                        groupHeaderRow.style.justifyContent = Justify.SpaceBetween;
                        groupHeaderRow.style.marginBottom = 6;
                        groupHeaderRow.style.paddingBottom = 4;
                        groupHeaderRow.style.borderBottomWidth = 1;
                        groupHeaderRow.style.borderBottomColor = new StyleColor(new Color(0.35f, 0.35f, 0.35f));

                        var groupLabel = new Label($"Command Group: {command.Name}");
                        groupLabel.style.fontSize = 11;
                        groupLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                        groupLabel.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
                        groupHeaderRow.Add(groupLabel);

                        var groupRunBtn = new Button(() => ExecuteStep(command)) { text = "Run All" };
                        groupRunBtn.style.width = 70;
                        groupHeaderRow.Add(groupRunBtn);

                        groupHeaderContainer.Add(groupHeaderRow);

                        // Display each nested command as separate line
                        if (nestedCommandsList.Count > 0)
                        {
                            var nestedItemsContainer = new VisualElement();
                            nestedItemsContainer.style.flexDirection = FlexDirection.Column;
                            nestedItemsContainer.style.marginLeft = 12;
                            nestedItemsContainer.style.borderLeftWidth = 2;
                            nestedItemsContainer.style.borderLeftColor = new StyleColor(new Color(0.4f, 0.6f, 0.8f));
                            nestedItemsContainer.style.paddingLeft = 8;

                            foreach (var nestedCmd in nestedCommandsList)
                            {
                                if (nestedCmd == null) continue;

                                var nestedItemContainer = new VisualElement();
                                nestedItemContainer.style.flexDirection = FlexDirection.Column;
                                nestedItemContainer.style.marginBottom = 8;
                                nestedItemContainer.style.paddingBottom = 8;
                                nestedItemContainer.style.borderBottomWidth = 1;
                                nestedItemContainer.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

                                // Nested command header line
                                var nestedHeaderRow = new VisualElement();
                                nestedHeaderRow.style.flexDirection = FlexDirection.Row;
                                nestedHeaderRow.style.alignItems = Align.Center;
                                nestedHeaderRow.style.justifyContent = Justify.SpaceBetween;
                                nestedHeaderRow.style.marginBottom = 6;

                                var nestedToggle = new Toggle();
                                nestedToggle.value = nestedCmd.IsActive;
                                nestedToggle.style.marginRight = 6;
                                nestedToggle.RegisterValueChangedCallback(evt =>
                                {
                                    var field = nestedCmd.GetType().GetField("isActive", BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                                    if (field != null)
                                    {
                                        field.SetValue(nestedCmd, evt.newValue);
                                        EditorUtility.SetDirty(_selectedPipeline);
                                    }
                                });
                                nestedHeaderRow.Add(nestedToggle);

                                var nestedNameLabel = new Label(nestedCmd.Name);
                                nestedNameLabel.style.fontSize = 10;
                                nestedNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                                nestedNameLabel.style.flexGrow = 1;
                                nestedHeaderRow.Add(nestedNameLabel);

                                var nestedTypeLabel = new Label(nestedCmd.GetType().Name);
                                nestedTypeLabel.style.fontSize = 8;
                                nestedTypeLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                                nestedTypeLabel.style.marginRight = 8;
                                nestedHeaderRow.Add(nestedTypeLabel);

                                var nestedExecuteBtn = new Button(() => ExecuteStep(nestedCmd)) { text = "Run" };
                                nestedExecuteBtn.style.width = 45;
                                nestedExecuteBtn.style.fontSize = 9;
                                nestedHeaderRow.Add(nestedExecuteBtn);

                                nestedItemContainer.Add(nestedHeaderRow);

                                // Nested command properties inline (no foldout)
                                var nestedPropsContainer = new VisualElement();
                                nestedPropsContainer.style.flexDirection = FlexDirection.Column;
                                nestedPropsContainer.style.marginLeft = 4;
                                nestedPropsContainer.style.paddingLeft = 4;
                                nestedPropsContainer.style.paddingRight = 4;
                                DisplayCommandProperties(nestedCmd, nestedPropsContainer);
                                
                                if (nestedPropsContainer.childCount > 0)
                                    nestedItemContainer.Add(nestedPropsContainer);

                                nestedItemsContainer.Add(nestedItemContainer);
                            }

                            groupHeaderContainer.Add(nestedItemsContainer);
                        }
                        else
                        {
                            var emptyLabel = new Label("No nested commands");
                            emptyLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                            emptyLabel.style.fontSize = 9;
                            emptyLabel.style.marginLeft = 12;
                            groupHeaderContainer.Add(emptyLabel);
                        }

                        contentContainer.Add(groupHeaderContainer);
                    }
                    else
                    {
                        // For multiple regular commands, create wrapper container (column layout) with no extra indentation
                        var commandWrapper = new VisualElement();
                        commandWrapper.style.flexDirection = FlexDirection.Column;
                        commandWrapper.style.marginLeft = 0;
                        commandWrapper.style.marginRight = 0;
                        commandWrapper.style.marginTop = 4;
                        commandWrapper.style.marginBottom = 4;

                        // Header line: toggle, type, buttons (no name label - already in foldout header)
                        var commandHeaderRow = new VisualElement();
                        commandHeaderRow.style.flexDirection = FlexDirection.Row;
                        commandHeaderRow.style.alignItems = Align.Center;
                        commandHeaderRow.style.justifyContent = Justify.SpaceBetween;
                        commandHeaderRow.style.marginBottom = 0;
                        commandHeaderRow.style.paddingLeft = 4;
                        commandHeaderRow.style.paddingRight = 4;

                        // Left section: toggle, type label
                        var leftSection = new VisualElement();
                        leftSection.style.flexDirection = FlexDirection.Row;
                        leftSection.style.alignItems = Align.Center;
                        leftSection.style.flexGrow = 1;

                        var toggle = new Toggle();
                        toggle.value = command.IsActive;
                        toggle.style.marginRight = 6;
                        toggle.RegisterValueChangedCallback(evt =>
                        {
                            var field = command.GetType().GetField("isActive", BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                            if (field != null)
                            {
                                field.SetValue(command, evt.newValue);
                                EditorUtility.SetDirty(_selectedPipeline);
                            }
                        });
                        leftSection.Add(toggle);

                        var typeLabel = new Label(command.GetType().Name);
                        typeLabel.style.fontSize = 9;
                        typeLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                        typeLabel.style.marginRight = 8;
                        leftSection.Add(typeLabel);

                        // Right section: buttons
                        var rightSection = new VisualElement();
                        rightSection.style.flexDirection = FlexDirection.Row;
                        rightSection.style.alignItems = Align.Center;

                        var executeBtn = new Button(() => ExecuteStep(command)) { text = "Run" };
                        executeBtn.style.width = 50;
                        executeBtn.style.marginRight = 4;
                        rightSection.Add(executeBtn);

                        var removeBtn = new Button(() => RemoveStep(step)) { text = "Remove" };
                        removeBtn.style.width = 70;
                        removeBtn.AddToClassList("button-danger");
                        rightSection.Add(removeBtn);

                        commandHeaderRow.Add(leftSection);
                        commandHeaderRow.Add(rightSection);

                        commandWrapper.Add(commandHeaderRow);

                        // Command properties below header (no extra indentation)
                        var inspectorContainer = new VisualElement();
                        inspectorContainer.style.flexDirection = FlexDirection.Column;
                        inspectorContainer.style.paddingLeft = 4;
                        inspectorContainer.style.paddingTop = 0;
                        inspectorContainer.style.paddingRight = 4;
                        inspectorContainer.style.marginLeft = 0;
                        inspectorContainer.style.marginRight = 0;
                        DisplayCommandProperties(command, inspectorContainer);
                        
                        if (inspectorContainer.childCount > 0)
                        {
                            commandWrapper.Add(inspectorContainer);
                        }

                        contentContainer.Add(commandWrapper);
                    }
                }
            }

            stepFoldout.Add(contentContainer);
            container.Add(stepFoldout);
            return container;
        }

        private void DisplayCommandProperties(IUnityBuildCommand command, VisualElement container)
        {
            if (command == null)
                return;

            var commandType = command.GetType();
            var fields = commandType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            int propertyCount = 0;

            foreach (var fieldInfo in fields)
            {
                // Skip backing fields and internal Unity fields
                if (fieldInfo.Name.StartsWith("<") || fieldInfo.Name.StartsWith("m_"))
                    continue;

                // Skip isActive as it's handled by toggle
                if (fieldInfo.Name == "isActive")
                    continue;

                propertyCount++;

                // Get field value
                var fieldValue = fieldInfo.GetValue(command);

                // Create field container
                var fieldContainer = new VisualElement();
                fieldContainer.style.flexDirection = FlexDirection.Row;
                fieldContainer.style.alignItems = Align.Center;
                fieldContainer.style.justifyContent = Justify.SpaceBetween;
                fieldContainer.style.paddingLeft = 4;
                fieldContainer.style.paddingRight = 4;
                fieldContainer.style.paddingTop = 3;
                fieldContainer.style.paddingBottom = 3;
                fieldContainer.style.marginBottom = 4;
                fieldContainer.style.borderBottomWidth = 1;
                fieldContainer.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

                // Field label
                var fieldNameLabel = new Label(fieldInfo.Name);
                fieldNameLabel.style.fontSize = 9;
                fieldNameLabel.style.minWidth = 120;
                fieldNameLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
                fieldContainer.Add(fieldNameLabel);

                // Field editor based on type
                VisualElement fieldEditor = null;
                
                if (fieldInfo.FieldType == typeof(string))
                {
                    var textField = new TextField();
                    textField.value = (string)fieldValue ?? "";
                    textField.style.flexGrow = 1;
                    textField.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = textField;
                }
                else if (fieldInfo.FieldType == typeof(int))
                {
                    var intField = new IntegerField();
                    intField.value = (int)fieldValue;
                    intField.style.flexGrow = 1;
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = intField;
                }
                else if (fieldInfo.FieldType == typeof(float))
                {
                    var floatField = new FloatField();
                    floatField.value = (float)fieldValue;
                    floatField.style.flexGrow = 1;
                    floatField.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = floatField;
                }
                else if (fieldInfo.FieldType == typeof(bool))
                {
                    var boolField = new Toggle();
                    boolField.value = (bool)fieldValue;
                    boolField.style.flexGrow = 1;
                    boolField.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = boolField;
                }
                else if (fieldInfo.FieldType == typeof(Vector2))
                {
                    var v2Field = new Vector2Field();
                    v2Field.value = (Vector2)fieldValue;
                    v2Field.style.flexGrow = 1;
                    v2Field.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = v2Field;
                }
                else if (fieldInfo.FieldType == typeof(Vector3))
                {
                    var v3Field = new Vector3Field();
                    v3Field.value = (Vector3)fieldValue;
                    v3Field.style.flexGrow = 1;
                    v3Field.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = v3Field;
                }
                else if (fieldInfo.FieldType == typeof(Vector4))
                {
                    var v4Field = new Vector4Field();
                    v4Field.value = (Vector4)fieldValue;
                    v4Field.style.flexGrow = 1;
                    v4Field.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = v4Field;
                }
                else if (fieldInfo.FieldType == typeof(Color))
                {
                    var colorField = new ColorField();
                    colorField.value = (Color)fieldValue;
                    colorField.style.flexGrow = 1;
                    colorField.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = colorField;
                }
                else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
                {
                    var objField = new ObjectField();
                    objField.objectType = fieldInfo.FieldType;
                    objField.value = fieldValue as UnityEngine.Object;
                    objField.style.flexGrow = 1;
                    objField.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = objField;
                }
                else if (fieldInfo.FieldType.IsEnum)
                {
                    var enumField = new EnumField((System.Enum)fieldValue);
                    enumField.style.flexGrow = 1;
                    enumField.RegisterValueChangedCallback(evt =>
                    {
                        fieldInfo.SetValue(command, evt.newValue);
                        EditorUtility.SetDirty(_selectedPipeline);
                    });
                    fieldEditor = enumField;
                }
                else if (IsSerializableType(fieldInfo.FieldType))
                {
                    // For serializable types, create a sub-container with nested fields
                    fieldEditor = null; // Will be handled specially below
                }
                else
                {
                    // For unsupported types, show read-only label
                    var valueLabel = new Label(fieldValue?.ToString() ?? "null");
                    valueLabel.style.fontSize = 9;
                    valueLabel.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
                    valueLabel.style.flexGrow = 1;
                    fieldEditor = valueLabel;
                }

                if (fieldEditor != null)
                {
                    fieldEditor.style.minWidth = 150;
                    fieldContainer.Add(fieldEditor);
                }

                container.Add(fieldContainer);

                // For serializable types, display their nested fields
                if (IsSerializableType(fieldInfo.FieldType) && fieldValue != null)
                {
                    var nestedFields = fieldInfo.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    
                    if (nestedFields.Length > 0)
                    {
                        var nestedContainer = new VisualElement();
                        nestedContainer.style.flexDirection = FlexDirection.Column;
                        nestedContainer.style.paddingLeft = 20;
                        nestedContainer.style.marginLeft = 8;
                        nestedContainer.style.borderLeftWidth = 1;
                        nestedContainer.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));

                        foreach (var nestedFieldInfo in nestedFields)
                        {
                            // Skip backing fields
                            if (nestedFieldInfo.Name.StartsWith("<") || nestedFieldInfo.Name.StartsWith("m_"))
                                continue;

                            var nestedValue = nestedFieldInfo.GetValue(fieldValue);

                            var nestedFieldContainer = new VisualElement();
                            nestedFieldContainer.style.flexDirection = FlexDirection.Row;
                            nestedFieldContainer.style.alignItems = Align.Center;
                            nestedFieldContainer.style.justifyContent = Justify.SpaceBetween;
                            nestedFieldContainer.style.paddingLeft = 4;
                            nestedFieldContainer.style.paddingRight = 4;
                            nestedFieldContainer.style.paddingTop = 3;
                            nestedFieldContainer.style.paddingBottom = 3;
                            nestedFieldContainer.style.marginBottom = 4;
                            nestedFieldContainer.style.borderBottomWidth = 1;
                            nestedFieldContainer.style.borderBottomColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));

                            var nestedLabel = new Label(nestedFieldInfo.Name);
                            nestedLabel.style.fontSize = 9;
                            nestedLabel.style.minWidth = 120;
                            nestedLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                            nestedFieldContainer.Add(nestedLabel);

                            VisualElement nestedEditor = null;

                            if (nestedFieldInfo.FieldType == typeof(string))
                            {
                                var textField = new TextField();
                                textField.value = (string)nestedValue ?? "";
                                textField.style.flexGrow = 1;
                                textField.RegisterValueChangedCallback(evt =>
                                {
                                    nestedFieldInfo.SetValue(fieldValue, evt.newValue);
                                    EditorUtility.SetDirty(_selectedPipeline);
                                });
                                nestedEditor = textField;
                            }
                            else if (nestedFieldInfo.FieldType == typeof(int))
                            {
                                var intField = new IntegerField();
                                intField.value = (int)nestedValue;
                                intField.style.flexGrow = 1;
                                intField.RegisterValueChangedCallback(evt =>
                                {
                                    nestedFieldInfo.SetValue(fieldValue, evt.newValue);
                                    EditorUtility.SetDirty(_selectedPipeline);
                                });
                                nestedEditor = intField;
                            }
                            else if (nestedFieldInfo.FieldType == typeof(float))
                            {
                                var floatField = new FloatField();
                                floatField.value = (float)nestedValue;
                                floatField.style.flexGrow = 1;
                                floatField.RegisterValueChangedCallback(evt =>
                                {
                                    nestedFieldInfo.SetValue(fieldValue, evt.newValue);
                                    EditorUtility.SetDirty(_selectedPipeline);
                                });
                                nestedEditor = floatField;
                            }
                            else if (nestedFieldInfo.FieldType == typeof(bool))
                            {
                                var boolField = new Toggle();
                                boolField.value = (bool)nestedValue;
                                boolField.style.flexGrow = 1;
                                boolField.RegisterValueChangedCallback(evt =>
                                {
                                    nestedFieldInfo.SetValue(fieldValue, evt.newValue);
                                    EditorUtility.SetDirty(_selectedPipeline);
                                });
                                nestedEditor = boolField;
                            }
                            else if (nestedFieldInfo.FieldType.IsEnum)
                            {
                                var enumField = new EnumField((System.Enum)nestedValue);
                                enumField.style.flexGrow = 1;
                                enumField.RegisterValueChangedCallback(evt =>
                                {
                                    nestedFieldInfo.SetValue(fieldValue, evt.newValue);
                                    EditorUtility.SetDirty(_selectedPipeline);
                                });
                                nestedEditor = enumField;
                            }
                            else if (typeof(UnityEngine.Object).IsAssignableFrom(nestedFieldInfo.FieldType))
                            {
                                var objField = new ObjectField();
                                objField.objectType = nestedFieldInfo.FieldType;
                                objField.value = nestedValue as UnityEngine.Object;
                                objField.style.flexGrow = 1;
                                objField.RegisterValueChangedCallback(evt =>
                                {
                                    nestedFieldInfo.SetValue(fieldValue, evt.newValue);
                                    EditorUtility.SetDirty(_selectedPipeline);
                                });
                                nestedEditor = objField;
                            }
                            else
                            {
                                var valueLabel = new Label(nestedValue?.ToString() ?? "null");
                                valueLabel.style.fontSize = 9;
                                valueLabel.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
                                valueLabel.style.flexGrow = 1;
                                nestedEditor = valueLabel;
                            }

                            if (nestedEditor != null)
                            {
                                nestedEditor.style.minWidth = 150;
                                nestedFieldContainer.Add(nestedEditor);
                            }

                            nestedContainer.Add(nestedFieldContainer);
                        }

                        container.Add(nestedContainer);
                    }
                }
            }

            // If no properties found, show a message
            if (propertyCount == 0)
            {
                var emptyLabel = new Label("No public fields");
                emptyLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                emptyLabel.style.fontSize = 9;
                container.Add(emptyLabel);
            }
        }

        private bool IsSerializableType(Type type)
        {
            // Check if type is marked with [Serializable] attribute
            if (type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
                return true;

            // Check if it's a custom class (not a primitive, string, or known unsupported types)
            if (type.IsValueType || type == typeof(string) || type.IsEnum || type.IsArray)
                return false;

            // If it's a class, check if it has SerializableAttribute
            return type.IsClass && !typeof(UnityEngine.Object).IsAssignableFrom(type);
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

            _selectedPipeline.preBuildCommands.Remove(step);
            _selectedPipeline.postBuildCommands.Remove(step);
            EditorUtility.SetDirty(_selectedPipeline);
            RefreshPipelineEditor();
            UpdateStatusLabel("Step removed");
        }

        private void PingSelectedPipeline()
        {
            if (_selectedPipeline == null)
            {
                UpdateStatusLabel("No pipeline selected");
                return;
            }

            EditorGUIUtility.PingObject(_selectedPipeline);
            UpdateStatusLabel($"Pinged: {_selectedPipeline.name}");
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

            // Execute pre-build commands
            foreach (var step in _selectedPipeline.preBuildCommands)
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

            // Execute post-build commands
            foreach (var step in _selectedPipeline.postBuildCommands)
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
            }                var executionTime = (float)(EditorApplication.timeSinceStartup - startTime);
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

                var pipeline = ScriptableObject.CreateInstance<PipelineCommandsGroup>();
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
