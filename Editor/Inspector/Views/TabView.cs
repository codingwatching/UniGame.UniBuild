namespace UniGame.UniBuild.Editor.Inspector.Views
{
    using System;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom TabView control for UI Toolkit
    /// Provides a tabbed interface for organizing content
    /// </summary>
    public class TabView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TabView, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private VisualElement _tabHeaderContainer;
        private VisualElement _tabContentContainer;
        private List<Tab> _tabs = new List<Tab>();
        private Tab _activeTab;

        public TabView()
        {
            style.flexGrow = 1;
            style.flexDirection = FlexDirection.Column;

            // Header container
            _tabHeaderContainer = new VisualElement();
            _tabHeaderContainer.AddToClassList("tab-header");
            _tabHeaderContainer.style.flexDirection = FlexDirection.Row;
            _tabHeaderContainer.style.borderBottomWidth = 1;
            _tabHeaderContainer.style.borderBottomColor = new StyleColor(new UnityEngine.Color(0.2f, 0.2f, 0.2f));
            Add(_tabHeaderContainer);

            // Content container
            _tabContentContainer = new VisualElement();
            _tabContentContainer.AddToClassList("tab-content");
            _tabContentContainer.style.flexGrow = 1;
            _tabContentContainer.style.flexDirection = FlexDirection.Column;
            Add(_tabContentContainer);
        }

        public void Add(Tab tab)
        {
            _tabs.Add(tab);

            // Create header button
            var headerButton = new Button(() => SelectTab(tab));
            headerButton.text = tab.label;
            headerButton.AddToClassList("tab-button");
            _tabHeaderContainer.Add(headerButton);

            // Store reference to button for later updating
            tab._headerButton = headerButton;

            // If this is the first tab, select it
            if (_tabs.Count == 1)
            {
                SelectTab(tab);
            }
        }

        private void SelectTab(Tab tab)
        {
            if (_activeTab != null)
            {
                _activeTab._headerButton.RemoveFromClassList("active");
                _activeTab.content.style.display = DisplayStyle.None;
            }

            _activeTab = tab;
            _activeTab._headerButton.AddToClassList("active");
            _activeTab.content.style.display = DisplayStyle.Flex;

            // Update content
            _tabContentContainer.Clear();
            _tabContentContainer.Add(_activeTab.content);
        }

        /// <summary>
        /// Represents a single tab in the TabView
        /// </summary>
        public class Tab
        {
            public string label;
            public VisualElement content;
            public Button _headerButton;

            public Tab()
            {
                label = "Tab";
                content = new VisualElement();
            }
        }
    }
}
