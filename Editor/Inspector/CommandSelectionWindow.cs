namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniModules.UniGame.UniBuild;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Window for selecting which command type to add to a pipeline or command group
    /// Displays all available commands grouped by category
    /// </summary>
    public class CommandSelectionWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "Select Command";
        private TextField _searchField;
        private ScrollView _commandListScrollView;
        private Dictionary<Type, BuildCommandMetadataAttribute> _commandMetadata;
        private Action<Type> _onCommandSelected;
        private string _searchFilter = "";

        public static void ShowWindow(Action<Type> onCommandSelected)
        {
            var window = GetWindow<CommandSelectionWindow>();
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.minSize = new Vector2(400, 500);
            window._onCommandSelected = onCommandSelected;
        }

        private void OnEnable()
        {
            CollectCommandMetadata();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            // Search field
            _searchField = new TextField("Search Commands:");
            _searchField.style.marginBottom = 8;
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _searchFilter = evt.newValue.ToLower();
                RefreshCommandList();
            });
            root.Add(_searchField);

            // Command list
            _commandListScrollView = new ScrollView();
            _commandListScrollView.style.flexGrow = 1;
            root.Add(_commandListScrollView);

            RefreshCommandList();
        }

        private void CollectCommandMetadata()
        {
            _commandMetadata = CommandDiscovery.GetAllCommandsWithMetadata();
        }

        private void RefreshCommandList()
        {
            _commandListScrollView.Clear();

            if (_commandMetadata.Count == 0)
            {
                _commandListScrollView.Add(new Label("No commands found"));
                return;
            }

            // Group commands by category
            var groups = _commandMetadata
                .Where(kvp =>
                {
                    var displayName = kvp.Value.DisplayName.ToLower();
                    var description = kvp.Value.Description.ToLower();
                    return displayName.Contains(_searchFilter) || description.Contains(_searchFilter);
                })
                .GroupBy(kvp => kvp.Value.Category)
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                // Category header
                var categoryFoldout = new Foldout
                {
                    text = group.Key ?? "Uncategorized",
                    value = true
                };
                categoryFoldout.style.fontSize = 12;
                categoryFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
                categoryFoldout.style.marginBottom = 4;

                // Commands in category
                foreach (var kvp in group.OrderBy(x => x.Value.DisplayName))
                {
                    var commandType = kvp.Key;
                    var metadata = kvp.Value;

                    var commandButton = CreateCommandButton(commandType, metadata);
                    categoryFoldout.Add(commandButton);
                }

                _commandListScrollView.Add(categoryFoldout);
            }
        }

        private VisualElement CreateCommandButton(Type commandType, BuildCommandMetadataAttribute metadata)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.paddingLeft = 8;
            container.style.paddingRight = 8;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 4;
            container.style.marginBottom = 4;
            container.style.borderTopWidth = 1;
            container.style.borderBottomWidth = 1;
            container.style.borderLeftWidth = 1;
            container.style.borderRightWidth = 1;
            container.style.borderTopColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            container.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            container.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            container.style.borderRightColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));

            // Make it clickable
            container.RegisterCallback<MouseUpEvent>(evt =>
            {
                _onCommandSelected?.Invoke(commandType);
                Close();
            });

            // Hover effect
            container.RegisterCallback<MouseEnterEvent>(evt =>
            {
                container.style.backgroundColor = new StyleColor(new Color(0.3f, 0.45f, 0.6f, 0.3f));
            });
            container.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                container.style.backgroundColor = new StyleColor(Color.clear);
            });

            // Title
            var titleLabel = new Label(metadata.DisplayName);
            titleLabel.style.fontSize = 12;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 2;
            container.Add(titleLabel);

            // Description
            var descriptionLabel = new Label(metadata.Description);
            descriptionLabel.style.fontSize = 10;
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            descriptionLabel.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            container.Add(descriptionLabel);

            // Type info
            var typeLabel = new Label($"Type: {commandType.Name}");
            typeLabel.style.fontSize = 9;
            typeLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            typeLabel.style.marginTop = 2;
            container.Add(typeLabel);

            return container;
        }
    }
}
