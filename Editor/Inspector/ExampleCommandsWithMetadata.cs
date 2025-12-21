namespace UniGame.UniBuild.Editor.Inspector.Examples
{
    using System.Collections.Generic;
    using UniGame.UniBuild.Editor;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    // ============== Platform Configuration Commands ==============

    [BuildCommandMetadata(
        displayName: "Switch Build Target (Android)",
        description: "Переключает активную платформу сборки на Android. Требуется установленный Android SDK.",
        category: "Platform Configuration"
    )]
    public class ExampleSwitchToAndroidCommand : SerializableBuildCommand
    {
        [SerializeField]
        private bool switchGraphics = true;

        public override string Name => "Switch to Android";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android,
                BuildTarget.Android
            );

            if (switchGraphics)
            {
                // Additional Android-specific settings
            }
        }
    }

    [BuildCommandMetadata(
        displayName: "Switch Build Target (iOS)",
        description: "Переключает активную платформу сборки на iOS. Требуется устройство на macOS.",
        category: "Platform Configuration"
    )]
    public class ExampleSwitchToIOSCommand : SerializableBuildCommand
    {
        [SerializeField]
        private bool switchGraphics = true;

        public override string Name => "Switch to iOS";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.iOS,
                BuildTarget.iOS
            );

            if (switchGraphics)
            {
                // Additional iOS-specific settings
            }
        }
    }

    // ============== Build Configuration Commands ==============

    [BuildCommandMetadata(
        displayName: "Set Scripting Define Symbols",
        description: "Устанавливает глобальные символы препроцессора для текущей платформы. " +
                     "Используется для условной компиляции кода.",
        category: "Build Configuration"
    )]
    public class ExampleSetDefineSymbolsCommand : SerializableBuildCommand
    {
        [SerializeField]
        private string defineSymbols = "DEVELOPMENT_BUILD;ENABLE_LOGGING";

        public override string Name => "Set Define Symbols";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, defineSymbols);
        }
    }

    [BuildCommandMetadata(
        displayName: "Set Managed Stripping Level",
        description: "Настраивает уровень удаления неиспользуемого кода в сборке. " +
                     "Повышает уровень для уменьшения размера сборки.",
        category: "Build Configuration"
    )]
    public class ExampleSetManagedStrippingCommand : SerializableBuildCommand
    {
        [SerializeField]
        private ManagedStrippingLevel strippingLevel = ManagedStrippingLevel.High;

        public override string Name => "Set Managed Stripping Level";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            PlayerSettings.SetManagedStrippingLevel(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                strippingLevel
            );
        }
    }

    // ============== Asset Management Commands ==============

    [BuildCommandMetadata(
        displayName: "Build Player",
        description: "Выполняет сборку плеера для текущей платформы. " +
                     "Создает исполняемый файл или APK/IPA в зависимости от платформы.",
        category: "Build"
    )]
    public class ExampleBuildPlayerCommand : SerializableBuildCommand
    {
        [SerializeField]
        private string buildPath = "Builds/Game";

        [SerializeField]
        private BuildOptions buildOptions = BuildOptions.None;

        public override string Name => "Build Player";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            var scenes = EditorBuildSettings.scenes;
            var scenePaths = new List<string>();

            foreach (var scene in scenes)
            {
                scenePaths.Add(scene.path);
            }

            BuildPipeline.BuildPlayer(
                scenePaths.ToArray(),
                buildPath,
                EditorUserBuildSettings.activeBuildTarget,
                buildOptions
            );
        }
    }

    [BuildCommandMetadata(
        displayName: "Refresh Assets",
        description: "Обновляет базу данных ассетов Unity, переимпортирует все измененные файлы.",
        category: "Assets"
    )]
    public class ExampleRefreshAssetsCommand : SerializableBuildCommand
    {
        public override string Name => "Refresh Assets";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            AssetDatabase.Refresh();
        }
    }

    // ============== Versioning Commands ==============

    [BuildCommandMetadata(
        displayName: "Update Version",
        description: "Обновляет версию приложения в PlayerSettings. " +
                     "Увеличивает номер версии или обновляет код сборки.",
        category: "Versioning"
    )]
    public class ExampleUpdateVersionCommand : SerializableBuildCommand
    {
        [SerializeField]
        private bool incrementBuildNumber = true;

        public override string Name => "Update Version";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            if (incrementBuildNumber)
            {
                var version = PlayerSettings.bundleVersion;
                // Parse and increment version logic here
                Debug.Log($"Current version: {version}");
            }
        }
    }

    // ============== Notification Commands ==============

    [BuildCommandMetadata(
        displayName: "Log Build Info",
        description: "Выводит информацию о конфигурации сборки в консоль. " +
                     "Полезна для отладки параметров сборки.",
        category: "Utilities"
    )]
    public class ExampleLogBuildInfoCommand : SerializableBuildCommand
    {
        public override string Name => "Log Build Info";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            Debug.Log("=== Build Information ===");
            Debug.Log($"Build Target: {EditorUserBuildSettings.activeBuildTarget}");
            Debug.Log($"Build Target Group: {EditorUserBuildSettings.selectedBuildTargetGroup}");
            Debug.Log($"Version: {PlayerSettings.bundleVersion}");
            Debug.Log($"Bundle ID: {PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup)}");
        }
    }

    // ============== Cleanup Commands ==============

    [BuildCommandMetadata(
        displayName: "Clear Build Cache",
        description: "Очищает кэш сборки Unity. Полезна для полной пересборки.",
        category: "Cleanup"
    )]
    public class ExampleClearBuildCacheCommand : SerializableBuildCommand
    {
        public override string Name => "Clear Build Cache";

        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            UnityEngine.Device.SystemInfo.unsupportedIdentifier.ToString();
            // Clearing build cache logic
            Debug.Log("Build cache would be cleared here");
        }
    }
}
