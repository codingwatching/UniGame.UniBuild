namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Factory for creating consistently styled UI elements
    /// Eliminates repetitive styling code and ensures consistent appearance
    /// </summary>
    public static class UIElementFactory
    {
        /// <summary>
        /// Create a container with standard styling
        /// </summary>
        public static VisualElement CreateContainer(FlexDirection direction = FlexDirection.Column)
        {
            var container = new VisualElement();
            container.style.flexDirection = direction;
            return container;
        }

        /// <summary>
        /// Create a bordered container with alternating background color
        /// </summary>
        public static VisualElement CreateAlternatingBgContainer(int index, bool includeBorder = true)
        {
            var container = CreateContainer();
            var bgColor = index % 2 == 0 
                ? UIThemeConstants.Colors.BgAlternateEven 
                : UIThemeConstants.Colors.BgAlternateOdd;
            
            container.style.backgroundColor = new StyleColor(bgColor);
            container.style.paddingLeft = UIThemeConstants.Spacing.SmallPadding;
            container.style.paddingRight = UIThemeConstants.Spacing.SmallPadding;
            container.style.paddingTop = UIThemeConstants.Spacing.SmallMargin;
            container.style.paddingBottom = 2;
            container.style.marginBottom = 2;
            
            if (includeBorder)
            {
                container.style.borderBottomWidth = UIThemeConstants.Sizes.BorderWidth;
                container.style.borderBottomColor = new StyleColor(UIThemeConstants.Colors.BorderDefault);
            }
            
            return container;
        }

        /// <summary>
        /// Create a labeled container (row with label on left, content on right)
        /// </summary>
        public static VisualElement CreateLabeledRow(string labelText, out VisualElement contentArea, int minLabelWidth = UIThemeConstants.Sizes.LabelMinWidth)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.paddingLeft = UIThemeConstants.Spacing.SmallPadding;
            row.style.paddingRight = UIThemeConstants.Spacing.SmallPadding;
            row.style.paddingTop = 2;
            row.style.paddingBottom = 2;
            row.style.marginBottom = UIThemeConstants.Spacing.ItemMarginBottom;
            row.style.borderBottomWidth = UIThemeConstants.Sizes.BorderWidth;
            row.style.borderBottomColor = new StyleColor(UIThemeConstants.Colors.BorderDefault);

            var label = new Label(labelText);
            label.style.fontSize = UIThemeConstants.FontSizes.Small;
            label.style.minWidth = minLabelWidth;
            label.style.color = new StyleColor(UIThemeConstants.Colors.TextDimmed);
            row.Add(label);

            contentArea = new VisualElement();
            contentArea.style.flexGrow = 1;
            contentArea.style.minWidth = UIThemeConstants.Sizes.EditorMinWidth;
            row.Add(contentArea);

            return row;
        }

        /// <summary>
        /// Create a standard label with optional color
        /// </summary>
        public static Label CreateLabel(string text, int fontSize = UIThemeConstants.FontSizes.Small, Color? color = null)
        {
            var label = new Label(text);
            label.style.fontSize = fontSize;
            label.style.color = new StyleColor(color ?? UIThemeConstants.Colors.TextDimmed);
            return label;
        }

        /// <summary>
        /// Create a dimmed label (less visible)
        /// </summary>
        public static Label CreateDimmedLabel(string text, int fontSize = UIThemeConstants.FontSizes.Small)
        {
            return CreateLabel(text, fontSize, UIThemeConstants.Colors.TextVeryDimmed);
        }

        /// <summary>
        /// Create a button with standard styling
        /// </summary>
        public static Button CreateButton(string text, System.Action onClick, int width = 0)
        {
            var button = new Button(onClick) { text = text };
            if (width > 0)
                button.style.width = width;
            button.style.marginRight = UIThemeConstants.Spacing.Margin;
            
            // Prevent drag-drop from starting when clicking buttons
            button.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
            
            return button;
        }

        /// <summary>
        /// Create a small button (for row operations like up/down)
        /// </summary>
        public static Button CreateSmallButton(string text, System.Action onClick)
        {
            return CreateButton(text, onClick, UIThemeConstants.Sizes.ButtonSmall);
        }

        /// <summary>
        /// Create a button with danger styling (for delete operations)
        /// </summary>
        public static Button CreateDangerButton(string text, System.Action onClick, int width = UIThemeConstants.Sizes.ButtonLarge)
        {
            var button = CreateButton(text, onClick, width);
            button.AddToClassList("button-danger");
            return button;
        }

        /// <summary>
        /// Create a header row with buttons
        /// </summary>
        public static VisualElement CreateButtonRow(params (string text, System.Action onClick, int width)[] buttons)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.justifyContent = Justify.FlexEnd;
            row.style.paddingLeft = UIThemeConstants.Spacing.Padding;
            row.style.paddingRight = UIThemeConstants.Spacing.Padding;
            row.style.paddingTop = UIThemeConstants.Spacing.SmallPadding;
            row.style.paddingBottom = UIThemeConstants.Spacing.SmallPadding;
            row.style.marginBottom = 0;

            foreach (var (text, onClick, width) in buttons)
            {
                var button = CreateButton(text, onClick, width > 0 ? width : 50);
                row.Add(button);
            }

            return row;
        }

        /// <summary>
        /// Create an info box with borders
        /// </summary>
        public static VisualElement CreateInfoBox()
        {
            var box = new VisualElement();
            box.style.paddingLeft = UIThemeConstants.Spacing.Padding;
            box.style.paddingRight = UIThemeConstants.Spacing.Padding;
            box.style.paddingTop = UIThemeConstants.Spacing.Padding;
            box.style.paddingBottom = UIThemeConstants.Spacing.Padding;
            box.style.marginBottom = UIThemeConstants.Spacing.Padding;
            box.style.borderTopWidth = UIThemeConstants.Sizes.BorderWidth;
            box.style.borderRightWidth = UIThemeConstants.Sizes.BorderWidth;
            box.style.borderBottomWidth = UIThemeConstants.Sizes.BorderWidth;
            box.style.borderLeftWidth = UIThemeConstants.Sizes.BorderWidth;
            
            var borderColor = UIThemeConstants.Colors.BorderLight;
            box.style.borderTopColor = new StyleColor(borderColor);
            box.style.borderRightColor = new StyleColor(borderColor);
            box.style.borderBottomColor = new StyleColor(borderColor);
            box.style.borderLeftColor = new StyleColor(borderColor);

            return box;
        }

        /// <summary>
        /// Create a nested properties container
        /// </summary>
        public static VisualElement CreateNestedPropertiesContainer()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.paddingLeft = 20;
            container.style.marginLeft = UIThemeConstants.Spacing.Padding;
            container.style.borderLeftWidth = UIThemeConstants.Sizes.BorderWidth;
            container.style.borderLeftColor = new StyleColor(UIThemeConstants.Colors.BorderLight);
            return container;
        }

        /// <summary>
        /// Create a list items container (for foldout content)
        /// </summary>
        public static VisualElement CreateListItemsContainer()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.paddingLeft = 12;
            return container;
        }

        /// <summary>
        /// Create a selected pipeline item styling
        /// </summary>
        public static void ApplySelectedItemStyling(VisualElement element)
        {
            element.style.backgroundColor = new StyleColor(UIThemeConstants.Colors.BgSelectedStep);
            element.style.borderLeftWidth = UIThemeConstants.Sizes.BorderWidthThickSelection;
            element.style.borderLeftColor = new StyleColor(UIThemeConstants.Colors.BorderSelected);
        }

        /// <summary>
        /// Apply unselected item styling
        /// </summary>
        public static void ApplyUnselectedItemStyling(VisualElement element)
        {
            element.style.backgroundColor = new StyleColor(UIThemeConstants.Colors.BgUnselectedItem);
            element.style.borderLeftWidth = 0;
        }

        /// <summary>
        /// Create a drag visual clone element
        /// </summary>
        public static VisualElement CreateDragVisualClone(VisualElement sourceElement)
        {
            var clone = new VisualElement();
            clone.style.position = Position.Absolute;
            clone.style.backgroundColor = new StyleColor(UIThemeConstants.Colors.BgDragClone);
            clone.style.borderTopWidth = UIThemeConstants.Sizes.BorderWidthThick;
            clone.style.borderTopColor = new StyleColor(UIThemeConstants.Colors.BorderDragClone);
            clone.style.borderBottomWidth = UIThemeConstants.Sizes.BorderWidthThick;
            clone.style.borderBottomColor = new StyleColor(UIThemeConstants.Colors.BorderDragClone);
            clone.style.paddingLeft = UIThemeConstants.Spacing.Padding;
            clone.style.paddingRight = UIThemeConstants.Spacing.Padding;
            clone.style.paddingTop = UIThemeConstants.Spacing.SmallPadding;
            clone.style.paddingBottom = UIThemeConstants.Spacing.SmallPadding;
            clone.style.width = sourceElement.worldBound.width;
            clone.style.height = sourceElement.worldBound.height;
            clone.style.left = sourceElement.worldBound.x;
            clone.style.top = sourceElement.worldBound.y;

            var dragLabel = new Label("Перетаскивание...");
            dragLabel.style.fontSize = UIThemeConstants.FontSizes.Medium;
            dragLabel.style.color = new StyleColor(Color.white);
            dragLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            clone.Add(dragLabel);

            return clone;
        }
    }
}
