namespace UniGame.UniBuild.Editor.Inspector
{
    using System;
    using System.Linq;
    using UniModules.UniGame.UniBuild;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Примеры использования Build Pipeline Editor API
    /// Демонстрирует основные операции и паттерны работы
    /// </summary>
    public class BuildPipelineEditorUsageExamples
    {
        // ============== Пример 1: Загрузка и вывод всех пайплайнов ==============

        public static void Example_LoadAllPipelines()
        {
#if UNITY_EDITOR
            var pipelines = PipelineManager.LoadAllPipelines();
            
            Debug.Log($"Found {pipelines.Count} pipelines:");
            foreach (var pipeline in pipelines)
            {
                var path = PipelineManager.GetPipelinePath(pipeline);
                Debug.Log($"  - {pipeline.name} ({path})");
            }
#endif
        }

        // ============== Пример 2: Поиск пайплайна по имени ==============

        public static ScriptableCommandsGroup Example_SearchPipeline(string pipelineName)
        {
#if UNITY_EDITOR
            var pipelines = PipelineManager.LoadAllPipelines();
            var results = PipelineSearcher.SearchPipelinesByName(pipelines, pipelineName);
            
            if (results.Count > 0)
            {
                Debug.Log($"Found pipeline: {results[0].name}");
                return results[0];
            }
            else
            {
                Debug.LogWarning($"Pipeline '{pipelineName}' not found");
                return null;
            }
#else
            return null;
#endif
        }

        // ============== Пример 3: Создание нового пайплайна ==============

        public static void Example_CreateNewPipeline()
        {
#if UNITY_EDITOR
            var folderPath = "Assets/MyBuildPipelines";
            var pipeline = PipelineManager.CreatePipeline(folderPath, "ProductionBuild");
            
            Debug.Log($"Created pipeline: {pipeline.name}");
            EditorGUIUtility.PingObject(pipeline);
#endif
        }

        // ============== Пример 4: Выполнение пайплайна ==============

        public static void Example_ExecutePipeline()
        {
#if UNITY_EDITOR
            var pipelines = PipelineManager.LoadAllPipelines();
            if (pipelines.Count == 0)
            {
                Debug.LogError("No pipelines found");
                return;
            }

            var pipeline = pipelines[0];
            var executor = new PipelineExecutor(enableDetailedLogging: true);

            // Подписываемся на события
            executor.OnExecutionStarted += state =>
            {
                Debug.Log($"[Pipeline] Execution started: {state.PipelineName}");
            };

            executor.OnStepExecuted += stepName =>
            {
                Debug.Log($"[Pipeline] Step executed: {stepName}");
            };

            executor.OnStepFailed += (stepName, ex) =>
            {
                Debug.LogError($"[Pipeline] Step failed: {stepName} - {ex.Message}");
            };

            executor.OnExecutionCompleted += state =>
            {
                if (state.Success)
                {
                    Debug.Log($"[Pipeline] Pipeline completed successfully in {state.ExecutionTime:F2}s");
                }
                else
                {
                    Debug.LogError($"[Pipeline] Pipeline failed: {state.ErrorMessage}");
                }
            };

            // Выполняем пайплайн
            var executionState = executor.ExecutePipeline(pipeline);
#endif
        }

        // ============== Пример 5: Выполнение отдельного шага ==============

        public static void Example_ExecuteStep()
        {
#if UNITY_EDITOR
            var pipelines = PipelineManager.LoadAllPipelines();
            if (pipelines == null || pipelines.Count == 0)
            {
                Debug.LogError("No pipelines found");
                return;
            }

            var pipeline = pipelines[0];
            if (pipeline?.commands?.commands == null || pipeline.commands.commands.Count == 0)
            {
                Debug.LogError("No pipeline steps found");
                return;
            }

            var step = pipeline.commands.commands[0];
            var executor = new PipelineExecutor();

            // Get commands from the step
            var commands = step.GetCommands();
            foreach (var cmd in commands)
            {
                bool success = executor.ExecuteStep(cmd as SerializableBuildCommand);
                if (success)
                {
                    Debug.Log($"Step executed successfully: {cmd.Name}");
                }
                else
                {
                    Debug.LogError($"Step execution failed: {cmd.Name}");
                }
            }
#endif
        }

        // ============== Пример 6: Работа с командами через Discovery ==============

        public static void Example_DiscoverCommands()
        {
#if UNITY_EDITOR
            // Получить все доступные команды
            var allCommands = BuildCommandDiscovery.FindAllCommands();
            Debug.Log($"Total available commands: {allCommands.Count()}");

            // Получить команды по категориям
            var categories = BuildCommandDiscovery.GetAllCategories();
            foreach (var category in categories)
            {
                var commandsInCategory = BuildCommandDiscovery.FindCommandsByCategory(category);
                Debug.Log($"  Category '{category}': {commandsInCategory.Count()} commands");
            }

            // Поиск команд
            var androidCommands = BuildCommandDiscovery.SearchCommands("android");
            Debug.Log($"Android-related commands: {androidCommands.Count()}");
            foreach (var (cmdType, metadata) in androidCommands)
            {
                Debug.Log($"  - {metadata.DisplayName}: {metadata.Description}");
            }
#endif
        }

        // ============== Пример 7: Управление настройками ==============

        public static void Example_ManageSettings()
        {
#if UNITY_EDITOR
            var settings = BuildPipelineSettingsManager.GetSettings();

            Debug.Log($"Current settings:");
            Debug.Log($"  Pipeline Creation Path: {settings.PipelineCreationPath}");
            Debug.Log($"  Auto Refresh: {settings.AutoRefreshPipelines}");
            Debug.Log($"  Max History Size: {settings.MaxHistorySize}");
            Debug.Log($"  Show Descriptions: {settings.ShowCommandDescriptions}");
            Debug.Log($"  Detailed Logging: {settings.EnableDetailedLogging}");

            // Изменение настроек
            settings.PipelineCreationPath = "Assets/BuildPipelines";
            settings.AutoRefreshPipelines = true;
            settings.MaxHistorySize = 200;
            settings.EnableDetailedLogging = true;

            // Сохранение настроек
            BuildPipelineSettingsManager.SaveSettings(settings);
            Debug.Log("Settings saved");
#endif
        }

        // ============== Пример 8: Поиск пайплайнов по критериям ==============

        public static void Example_AdvancedSearch()
        {
#if UNITY_EDITOR
            var pipelines = PipelineManager.LoadAllPipelines();

            // Поиск по имени
            var namedPipelines = PipelineSearcher.SearchPipelinesByName(
                pipelines,
                "production"
            );
            Debug.Log($"Pipelines with 'production' in name: {namedPipelines.Count}");

            // Поиск по количеству шагов
            var largePipelines = PipelineSearcher.SearchPipelinesByStepCount(
                pipelines,
                minSteps: 5,
                maxSteps: 20
            );
            Debug.Log($"Pipelines with 5-20 steps: {largePipelines.Count}");

            // Получение статистики
            var stats = PipelineSearcher.GetStatistics(pipelines);
            Debug.Log($"Statistics: {stats}");
#endif
        }

        // ============== Пример 9: Дублирование пайплайна ==============

        public static void Example_DuplicatePipeline()
        {
#if UNITY_EDITOR
            var pipelines = PipelineManager.LoadAllPipelines();
            if (pipelines.Count == 0)
            {
                Debug.LogError("No pipelines found to duplicate");
                return;
            }

            var original = pipelines[0];
            var duplicate = PipelineManager.DuplicatePipeline(original);

            if (duplicate != null)
            {
                Debug.Log($"Duplicated: {original.name} -> {duplicate.name}");
                EditorGUIUtility.PingObject(duplicate);
            }
#endif
        }

        // ============== Пример 10: Удаление пайплайна ==============

        public static void Example_DeletePipeline()
        {
#if UNITY_EDITOR
            var pipelines = PipelineManager.LoadAllPipelines();
            var pipelineToDelete = pipelines.FirstOrDefault(p => p.name == "ToDelete");

            if (pipelineToDelete != null)
            {
                bool success = PipelineManager.DeletePipeline(pipelineToDelete);
                if (success)
                {
                    Debug.Log("Pipeline deleted");
                }
            }
#endif
        }

        // ============== Пример 11: Фильтрация шагов пайплайна ==============

        public static void Example_FilterPipelineSteps()
        {
#if UNITY_EDITOR
            var pipelines = PipelineManager.LoadAllPipelines();
            if (pipelines.Count == 0)
            {
                Debug.LogError("No pipelines found");
                return;
            }

            var pipeline = pipelines[0];

            // Поиск активных шагов
            var activeSteps = PipelineSearcher.SearchStepsByStatus(pipeline, activeOnly: true);
            Debug.Log($"Active steps in '{pipeline.name}': {activeSteps.Count}");

            // Поиск по имени
            var foundSteps = PipelineSearcher.SearchSteps(pipeline, "android");
            Debug.Log($"Steps with 'android': {foundSteps.Count}");
#endif
        }

        // ============== Пример 12: Получение информации о команде ==============

        public static void Example_GetCommandMetadata()
        {
#if UNITY_EDITOR
            var commands = BuildCommandDiscovery.FindAllCommands().ToList();
            
            if (commands.Count > 0)
            {
                var commandType = commands[0];
                var metadata = BuildCommandDiscovery.GetMetadata(commandType);

                Debug.Log($"Command: {metadata.DisplayName}");
                Debug.Log($"Description: {metadata.Description}");
                Debug.Log($"Category: {metadata.Category}");
                Debug.Log($"Type: {commandType.FullName}");
            }
#endif
        }
    }
}
