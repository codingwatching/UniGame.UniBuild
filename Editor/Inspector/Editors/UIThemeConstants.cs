namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using UnityEngine;

    /// <summary>
    /// Centralized UI theme and constants for the Build Pipeline Editor
    /// Eliminates magic numbers and provides consistent styling
    /// </summary>
    public static class UIThemeConstants
    {
        // ========== COLORS ==========
        
        public static class Colors
        {
            // Borders
            public static readonly Color BorderDefault = new Color(0.2f, 0.2f, 0.2f);
            public static readonly Color BorderLight = new Color(0.3f, 0.3f, 0.3f);
            public static readonly Color BorderDark = new Color(0.15f, 0.15f, 0.15f);
            
            // Backgrounds
            public static readonly Color BgAlternateEven = new Color(0.12f, 0.12f, 0.12f);
            public static readonly Color BgAlternateOdd = new Color(0.16f, 0.16f, 0.16f);
            public static readonly Color BgSelectedStep = new Color(0.2f, 0.4f, 0.6f, 0.8f);
            public static readonly Color BgUnselectedItem = new Color(0.15f, 0.15f, 0.15f);
            public static readonly Color BgHighlightDrop = new Color(0.3f, 0.5f, 0.8f, 0.2f);
            public static readonly Color BgDragClone = new Color(0.2f, 0.3f, 0.4f, 0.8f);
            
            // Text/Labels
            public static readonly Color TextDimmed = new Color(0.6f, 0.6f, 0.6f);
            public static readonly Color TextSemiDimmed = new Color(0.7f, 0.7f, 0.7f);
            public static readonly Color TextVeryDimmed = new Color(0.5f, 0.5f, 0.5f);
            
            // Accents
            public static readonly Color BorderSelected = new Color(0.4f, 0.8f, 1.0f);
            public static readonly Color BorderDragClone = new Color(0.4f, 0.6f, 0.9f);
        }
        
        // ========== SPACING ==========
        
        public static class Spacing
        {
            public const int Margin = 4;
            public const int SmallMargin = 2;
            public const int Padding = 8;
            public const int SmallPadding = 4;
            public const int ItemMarginBottom = 4;
            public const int ListItemHeight = 60;
        }
        
        // ========== SIZES ==========
        
        public static class Sizes
        {
            public const int SidebarWidth = 300;
            public const int BorderWidth = 1;
            public const int BorderWidthThick = 2;
            public const int BorderWidthThickSelection = 3;
            public const int ButtonSmall = 30;
            public const int ButtonMedium = 50;
            public const int ButtonLarge = 70;
            public const int ButtonXLarge = 100;
            public const int LabelMinWidth = 120;
            public const int EditorMinWidth = 150;
        }
        
        // ========== FONT SIZES ==========
        
        public static class FontSizes
        {
            public const int Small = 9;
            public const int Normal = 10;
            public const int Medium = 11;
            public const int Large = 12;
        }
        
        // ========== COMMON VALUES ==========
        
        public const string EmptyPipelineMessage = "No pipeline selected";
        public const string NoStepsMessage = "No steps added";
        public const string NoCommandsMessage = "No commands in this step";
        public const string NoPublicFieldsMessage = "No public fields";
    }
}
