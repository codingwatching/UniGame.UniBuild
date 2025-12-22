namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ClientBuild.BuildConfiguration;
    using UnityEditor;
    using UnityEditor.Build.Profile;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Renders build settings for a selected pipeline using reflection
    /// Supports ShowIf and HideIf attributes from Odin Inspector
    /// </summary>
    public class PipelineSettingsRenderer
    {
        private UniBuildPipeline _pipeline;
        private SerializedObject _serializedObject;
        private VisualElement _container;
        private Dictionary<string, VisualElement> _fieldElements = new Dictionary<string, VisualElement>();

        public PipelineSettingsRenderer(VisualElement container)
        {
            _container = container;
        }

        /// <summary>
        /// Refresh the settings display for a pipeline
        /// </summary>
        public void RefreshSettings(UniBuildPipeline pipeline)
        {
            _pipeline = pipeline;
            _container.Clear();
            _fieldElements.Clear();

            if (_pipeline == null)
            {
                _container.Add(new Label("No pipeline selected"));
                return;
            }

            _serializedObject = new SerializedObject(_pipeline);

            // Create main settings container
            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            _container.Add(scrollView);

            // Get all public fields from UniBuildConfigurationData
            var buildDataType = typeof(UniBuildConfigurationData);
            var fields = buildDataType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .ToList();

            // Sort fields to keep dependent fields next to their toggle
            var sortedFields = SortFieldsWithDependents(fields);

            var section = CreateSection("Build Configuration", scrollView);
            
            foreach (var field in sortedFields)
            {
                var fieldElement = CreateFieldElement(field);
                if (fieldElement != null)
                {
                    _fieldElements[field.Name] = fieldElement;
                    section.Add(fieldElement);
                    
                    // Set initial visibility based on ShowIf/HideIf attributes
                    bool shouldShow = ShouldShowField(field);
                    fieldElement.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
                }
                else
                {
                    Debug.LogWarning($"[PipelineSettings] Failed to create UI element for field: {field.Name} (type: {field.FieldType.Name})");
                }
            }

            // Also add Player Build section
            CreatePlayerBuildSection(scrollView);
        }

        /// <summary>
        /// Sort fields so dependent fields appear right after their toggle fields
        /// For example: overrideArtifactName comes before artifactName
        /// </summary>
        private List<FieldInfo> SortFieldsWithDependents(List<FieldInfo> fields)
        {
            var sortedList = new List<FieldInfo>();
            var processed = new HashSet<string>();
            
            // Create a map of field name -> ShowIf condition
            var showIfMap = new Dictionary<string, string>();
            foreach (var field in fields)
            {
                var showIfAttr = field.GetCustomAttribute<ShowIfAttribute>();
                if (showIfAttr != null)
                {
                    showIfMap[field.Name] = showIfAttr.Condition;
                }
            }

            // Create a reverse map: condition -> list of dependent fields
            var dependencyMap = new Dictionary<string, List<FieldInfo>>();
            foreach (var kvp in showIfMap)
            {
                if (!dependencyMap.ContainsKey(kvp.Value))
                {
                    dependencyMap[kvp.Value] = new List<FieldInfo>();
                }
                var field = fields.FirstOrDefault(f => f.Name == kvp.Key);
                if (field != null)
                {
                    dependencyMap[kvp.Value].Add(field);
                }
            }

            // Sort each dependency group
            foreach (var key in dependencyMap.Keys.ToList())
            {
                dependencyMap[key] = dependencyMap[key].OrderBy(f => f.Name).ToList();
            }

            // Build a set of field names that are themselves dependent on other fields
            var dependentFieldNames = new HashSet<string>(showIfMap.Keys);

            // Separate toggle fields into "root toggles" (not dependent on anything) and "dependent toggles"
            var allToggleFields = fields.Where(f => f.FieldType == typeof(bool)).OrderBy(f => f.Name).ToList();
            var rootToggleFields = allToggleFields.Where(f => !dependentFieldNames.Contains(f.Name)).ToList();
            
            // Process root toggle fields and their dependents recursively
            void ProcessToggleField(FieldInfo toggleField)
            {
                if (processed.Contains(toggleField.Name))
                    return;

                sortedList.Add(toggleField);
                processed.Add(toggleField.Name);

                // Add dependent fields right after
                if (dependencyMap.ContainsKey(toggleField.Name))
                {
                    foreach (var dependent in dependencyMap[toggleField.Name])
                    {
                        if (!processed.Contains(dependent.Name))
                        {
                            sortedList.Add(dependent);
                            processed.Add(dependent.Name);

                            // If dependent is also a toggle field, recursively process its dependents
                            if (dependent.FieldType == typeof(bool))
                            {
                                ProcessToggleField(dependent);
                            }
                        }
                    }
                }
            }

            // Process all root toggles (which will recursively process their dependent toggles)
            foreach (var toggleField in rootToggleFields)
            {
                ProcessToggleField(toggleField);
            }

            // Then add any remaining fields that weren't processed
            var nonToggleFields = fields.Where(f => f.FieldType != typeof(bool)).OrderBy(f => f.Name).ToList();
            foreach (var field in nonToggleFields)
            {
                if (!processed.Contains(field.Name))
                {
                    sortedList.Add(field);
                    processed.Add(field.Name);
                }
            }

            return sortedList;
        }

        /// <summary>
        /// Check if a field should be shown based on ShowIf/HideIf attributes
        /// </summary>
        private bool ShouldShowField(FieldInfo field)
        {
            // Check for ShowIf attribute
            var showIfAttr = field.GetCustomAttribute<ShowIfAttribute>();
            if (showIfAttr != null)
            {
                if (!EvaluateCondition(showIfAttr.Condition))
                    return false;
            }

            // Check for HideIf attribute
            var hideIfAttr = field.GetCustomAttribute<HideIfAttribute>();
            if (hideIfAttr != null)
            {
                if (EvaluateCondition(hideIfAttr.Condition))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Evaluate a condition like "nameof(overrideArtifactName)" or custom logic
        /// Supports fields, properties, and parameterless methods
        /// </summary>
        private bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            // First try to get value from SerializedObject (buildData)
            var buildDataProp = _serializedObject.FindProperty("buildData");
            if (buildDataProp != null)
            {
                var fieldProp = buildDataProp.FindPropertyRelative(condition);
                if (fieldProp != null && fieldProp.propertyType == SerializedPropertyType.Boolean)
                {
                    return fieldProp.boolValue;
                }
            }

            // Fallback to reflection if not found in SerializedObject
            var buildData = _pipeline.BuildData;
            if (buildData == null)
                return true;

            var type = buildData.GetType();

            // Try field first
            var field = type.GetField(condition, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null && field.FieldType == typeof(bool))
            {
                return (bool)field.GetValue(buildData);
            }

            // Try property (including computed properties like "IsWebGL")
            var property = type.GetProperty(condition, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null && property.PropertyType == typeof(bool) && property.CanRead)
            {
                try
                {
                    return (bool)property.GetValue(buildData);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[PipelineSettings] Failed to evaluate property '{condition}': {ex.Message}");
                    // If property getter fails, try method instead
                }
            }

            // Try method (for conditions like "IsWebGL()" or "IsShownStandaloneSubTarget()")
            var method = type.GetMethod(condition, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, Type.EmptyTypes, null);
            if (method != null && method.ReturnType == typeof(bool))
            {
                try
                {
                    return (bool)method.Invoke(buildData, null);
                }
                catch
                {
                    // If method invocation fails, default to true
                }
            }

            return true;
        }

        /// <summary>
        /// Create a UI element for a field
        /// </summary>
        private VisualElement CreateFieldElement(FieldInfo field)
        {
            var buildData = _pipeline.BuildData;
            var fieldType = field.FieldType;

            // Get the SerializedProperty for this field
            var buildDataProp = _serializedObject.FindProperty("buildData");
            var fieldProp = buildDataProp.FindPropertyRelative(field.Name);

            if (fieldProp == null)
                return null;

            var label = GetFieldLabel(field);
            var container = new VisualElement();
            container.style.marginBottom = 4;

            // Create appropriate field based on type
            if (fieldType == typeof(bool))
            {
                var toggle = new Toggle(label);
                toggle.value = fieldProp.boolValue;
                toggle.style.marginTop = 0;
                toggle.style.marginBottom = 0;
                toggle.style.paddingTop = 2;
                toggle.style.paddingBottom = 2;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    _serializedObject.Update();
                    buildDataProp.FindPropertyRelative(field.Name).boolValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_pipeline);
                    
                    // Sync serialized object back to runtime object
                    EditorUtility.SetDirty(_pipeline);
                    AssetDatabase.SaveAssets();
                    
                    // Update visibility of dependent fields
                    RefreshDependentFields();
                });
                container.Add(toggle);
            }
            else if (fieldType == typeof(string))
            {
                var textField = new TextField(label);
                textField.value = fieldProp.stringValue ?? "";
                textField.RegisterValueChangedCallback(evt =>
                {
                    _serializedObject.Update();
                    buildDataProp.FindPropertyRelative(field.Name).stringValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_pipeline);
                });
                container.Add(textField);
            }
            else if (fieldType == typeof(int))
            {
                var intField = new IntegerField(label);
                intField.value = fieldProp.intValue;
                intField.RegisterValueChangedCallback(evt =>
                {
                    _serializedObject.Update();
                    buildDataProp.FindPropertyRelative(field.Name).intValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_pipeline);
                });
                container.Add(intField);
            }
            else if (fieldType.IsEnum)
            {
                // Check if enum has [Flags] attribute
                bool isFlags = fieldType.GetCustomAttribute<System.FlagsAttribute>() != null;
                
                if (isFlags)
                {
                    // For flags enums, use enumValueFlag
                    var enumValue = (Enum)Enum.ToObject(fieldType, fieldProp.enumValueFlag);
                    var enumFlagsField = new EnumFlagsField(label, enumValue);
                    enumFlagsField.RegisterValueChangedCallback(evt =>
                    {
                        _serializedObject.Update();
                        buildDataProp.FindPropertyRelative(field.Name).enumValueFlag = (int)(object)evt.newValue;
                        _serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(_pipeline);
                        AssetDatabase.SaveAssets();
                        
                        // Refresh dependent fields in case this enum affects visibility
                        RefreshDependentFields();
                    });
                    container.Add(enumFlagsField);
                }
                else
                {
                    // For normal enums, use enumValueIndex
                    var enumValue = (Enum)Enum.ToObject(fieldType, fieldProp.enumValueIndex);
                    var enumField = new EnumField(label, enumValue);
                    enumField.RegisterValueChangedCallback(evt =>
                    {
                        try
                        {
                            _serializedObject.Update();
                            int newEnumValue = (int)(object)evt.newValue;
                            
                            // Re-fetch fresh buildDataProp after Update()
                            var freshBuildDataProp = _serializedObject.FindProperty("buildData");
                            if (freshBuildDataProp == null)
                                return;
                                
                            var freshFieldProp = freshBuildDataProp.FindPropertyRelative(field.Name);
                            if (freshFieldProp == null)
                                return;
                            
                            // Check if this is a valid enum index before setting
                            if (newEnumValue >= 0 && newEnumValue < fieldType.GetEnumNames().Length)
                            {
                                freshFieldProp.enumValueIndex = newEnumValue;
                                _serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(_pipeline);
                                AssetDatabase.SaveAssets();
                                
                                // Refresh dependent fields in case this enum affects visibility
                                RefreshDependentFields();
                            }
                            else
                            {
                                Debug.LogWarning($"[PipelineSettings] Enum value {newEnumValue} is out of range for {fieldType.Name}");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"[PipelineSettings] Error setting enum value: {ex.Message}");
                        }
                    });
                    container.Add(enumField);
                }
            }
            else if (fieldType == typeof(BuildProfile))
            {
                var objField = new ObjectField(label);
                objField.objectType = typeof(BuildProfile);
                objField.value = fieldProp.objectReferenceValue;
                objField.RegisterValueChangedCallback(evt =>
                {
                    _serializedObject.Update();
                    buildDataProp.FindPropertyRelative(field.Name).objectReferenceValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_pipeline);
                });
                container.Add(objField);
            }
            // For ArgumentsMap, use special handling
            else if (fieldType.Name == "ArgumentsMap")
            {
                return CreateArgumentsMapElement(field, fieldProp, label);
            }
            // For complex types like WebGlBuildData, show nested fields
            else if (!fieldType.IsValueType || fieldType.IsClass)
            {
                return CreateComplexFieldElement(field, fieldProp, label);
            }

            return container;
        }

        /// <summary>
        /// Create UI for complex nested objects
        /// </summary>
        private VisualElement CreateComplexFieldElement(FieldInfo field, SerializedProperty fieldProp, string label)
        {
            var container = new VisualElement();
            container.style.marginBottom = 4;
            container.style.paddingLeft = 16;
            container.style.paddingRight = 8;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 4;
            container.style.backgroundColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
            container.style.borderLeftWidth = 2;
            container.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));

            var titleLabel = new Label(label);
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 4;
            container.Add(titleLabel);

            var contentContainer = new VisualElement();
            contentContainer.style.flexDirection = FlexDirection.Column;

            // Get all public fields of the complex type
            var fieldType = field.FieldType;
            var nestedFields = fieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var nestedField in nestedFields)
            {
                var nestedProp = fieldProp.FindPropertyRelative(nestedField.Name);
                if (nestedProp == null)
                    continue;

                var nestedElement = CreateNestedFieldElement(nestedField, nestedProp);
                if (nestedElement != null)
                {
                    contentContainer.Add(nestedElement);
                }
            }

            container.Add(contentContainer);
            return container;
        }

        /// <summary>
        /// Create UI for a nested field
        /// </summary>
        private VisualElement CreateNestedFieldElement(FieldInfo field, SerializedProperty fieldProp)
        {
            var fieldType = field.FieldType;
            var label = GetFieldLabel(field);
            var container = new VisualElement();
            container.style.marginBottom = 3;

            if (fieldType == typeof(bool))
            {
                var toggle = new Toggle(label);
                toggle.value = fieldProp.boolValue;
                toggle.style.marginTop = 0;
                toggle.style.marginBottom = 0;
                toggle.style.paddingTop = 2;
                toggle.style.paddingBottom = 2;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    _serializedObject.Update();
                    fieldProp.boolValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_pipeline);
                });
                container.Add(toggle);
            }
            else if (fieldType == typeof(string))
            {
                var textField = new TextField(label);
                textField.value = fieldProp.stringValue ?? "";
                textField.style.marginTop = 0;
                textField.style.marginBottom = 0;
                textField.style.paddingTop = 2;
                textField.style.paddingBottom = 2;
                textField.RegisterValueChangedCallback(evt =>
                {
                    _serializedObject.Update();
                    fieldProp.stringValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_pipeline);
                });
                container.Add(textField);
            }
            else if (fieldType == typeof(int))
            {
                var intField = new IntegerField(label);
                intField.value = fieldProp.intValue;
                intField.style.marginTop = 0;
                intField.style.marginBottom = 0;
                intField.style.paddingTop = 2;
                intField.style.paddingBottom = 2;
                intField.RegisterValueChangedCallback(evt =>
                {
                    _serializedObject.Update();
                    fieldProp.intValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_pipeline);
                });
                container.Add(intField);
            }
            else if (fieldType.IsEnum)
            {
                // Check if enum has [Flags] attribute
                bool isFlags = fieldType.GetCustomAttribute<System.FlagsAttribute>() != null;
                
                if (isFlags)
                {
                    // For flags enums, use enumValueFlag
                    var enumValue = (Enum)Enum.ToObject(fieldType, fieldProp.enumValueFlag);
                    var enumFlagsField = new EnumFlagsField(label, enumValue);
                    enumFlagsField.RegisterValueChangedCallback(evt =>
                    {
                        _serializedObject.Update();
                        fieldProp.enumValueFlag = (int)(object)evt.newValue;
                        _serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(_pipeline);
                    });
                    container.Add(enumFlagsField);
                }
                else
                {
                    // For normal enums, use enumValueIndex
                    var enumValue = (Enum)Enum.ToObject(fieldType, fieldProp.enumValueIndex);
                    var enumField = new EnumField(label, enumValue);
                    enumField.RegisterValueChangedCallback(evt =>
                    {
                        _serializedObject.Update();
                        fieldProp.enumValueIndex = (int)(object)evt.newValue;
                        _serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(_pipeline);
                    });
                    container.Add(enumField);
                }
            }

            return container;
        }

        /// <summary>
        /// Create UI for ArgumentsMap with add/remove buttons for key-value pairs
        /// </summary>
        private VisualElement CreateArgumentsMapElement(FieldInfo field, SerializedProperty fieldProp, string label)
        {
            var container = new VisualElement();
            container.style.marginBottom = 4;
            container.style.paddingLeft = 16;
            container.style.paddingRight = 8;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 4;
            container.style.backgroundColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
            container.style.borderLeftWidth = 2;
            container.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));

            var titleContainer = new VisualElement();
            titleContainer.style.flexDirection = FlexDirection.Row;
            titleContainer.style.justifyContent = Justify.SpaceBetween;
            titleContainer.style.alignItems = Align.Center;
            titleContainer.style.marginBottom = 4;

            var titleLabel = new Label(label);
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleContainer.Add(titleLabel);

            var addButton = new Button(() => AddArgumentEntry(fieldProp))
            {
                text = "Add Argument"
            };
            addButton.style.paddingLeft = 8;
            addButton.style.paddingRight = 8;
            addButton.style.paddingTop = 4;
            addButton.style.paddingBottom = 4;
            titleContainer.Add(addButton);

            container.Add(titleContainer);

            // Get the arguments dictionary property
            var argumentsProp = fieldProp.FindPropertyRelative("arguments");
            if (argumentsProp == null)
                return container;

            var keysProp = argumentsProp.FindPropertyRelative("keys");
            var valuesProp = argumentsProp.FindPropertyRelative("values");

            if (keysProp == null || valuesProp == null)
                return container;

            // Display each key-value pair
            var entriesContainer = new VisualElement();
            entriesContainer.style.flexDirection = FlexDirection.Column;

            for (int i = 0; i < keysProp.arraySize; i++)
            {
                var entryContainer = new VisualElement();
                entryContainer.style.flexDirection = FlexDirection.Row;
                entryContainer.style.marginBottom = 3;
                entryContainer.style.paddingLeft = 4;
                entryContainer.style.paddingRight = 4;
                entryContainer.style.backgroundColor = new StyleColor(new Color(0.08f, 0.08f, 0.08f));

                var keyProp = keysProp.GetArrayElementAtIndex(i);
                var valueProp = valuesProp.GetArrayElementAtIndex(i);

                // Key field
                var keyField = new TextField("Key");
                keyField.value = keyProp.stringValue ?? "";
                keyField.style.flexGrow = 1;
                keyField.RegisterValueChangedCallback(evt =>
                {
                    _serializedObject.Update();
                    keyProp.stringValue = evt.newValue;
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_pipeline);
                });
                entryContainer.Add(keyField);

                // Value field (assuming BuildArgumentValue is a simple type or class)
                var valueField = new TextField("Value");
                valueField.value = GetArgumentValueString(valueProp);
                valueField.style.flexGrow = 1;
                valueField.RegisterValueChangedCallback(evt =>
                {
                    _serializedObject.Update();
                    SetArgumentValueString(valueProp, evt.newValue);
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_pipeline);
                });
                entryContainer.Add(valueField);

                // Remove button
                int index = i; // Capture for closure
                var removeButton = new Button(() => RemoveArgumentEntry(fieldProp, index))
                {
                    text = "Remove"
                };
                removeButton.style.paddingLeft = 8;
                removeButton.style.paddingRight = 8;
                removeButton.style.width = 80;
                entryContainer.Add(removeButton);

                entriesContainer.Add(entryContainer);
            }

            container.Add(entriesContainer);
            return container;
        }

        /// <summary>
        /// Get string representation of a BuildArgumentValue
        /// </summary>
        private string GetArgumentValueString(SerializedProperty valueProp)
        {
            // BuildArgumentValue might have nested fields - try to get the value
            var valuePropValue = valueProp.FindPropertyRelative("value");
            if (valuePropValue != null)
                return valuePropValue.stringValue ?? "";

            // Fallback: try stringValue directly
            if (valueProp.propertyType == SerializedPropertyType.String)
                return valueProp.stringValue ?? "";

            return "";
        }

        /// <summary>
        /// Set BuildArgumentValue from string
        /// </summary>
        private void SetArgumentValueString(SerializedProperty valueProp, string newValue)
        {
            var valuePropValue = valueProp.FindPropertyRelative("value");
            if (valuePropValue != null)
            {
                valuePropValue.stringValue = newValue;
                return;
            }

            // Fallback: try stringValue directly
            if (valueProp.propertyType == SerializedPropertyType.String)
            {
                valueProp.stringValue = newValue;
            }
        }

        /// <summary>
        /// Add a new key-value entry to ArgumentsMap
        /// </summary>
        private void AddArgumentEntry(SerializedProperty fieldProp)
        {
            var argumentsProp = fieldProp.FindPropertyRelative("arguments");
            if (argumentsProp == null)
                return;

            var keysProp = argumentsProp.FindPropertyRelative("keys");
            var valuesProp = argumentsProp.FindPropertyRelative("values");

            if (keysProp == null || valuesProp == null)
                return;

            _serializedObject.Update();

            keysProp.arraySize++;
            valuesProp.arraySize++;

            int newIndex = keysProp.arraySize - 1;
            keysProp.GetArrayElementAtIndex(newIndex).stringValue = "newKey";

            // Initialize the value
            var newValueProp = valuesProp.GetArrayElementAtIndex(newIndex);
            var valuePropValue = newValueProp.FindPropertyRelative("value");
            if (valuePropValue != null)
                valuePropValue.stringValue = "newValue";

            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_pipeline);
            AssetDatabase.SaveAssets();

            // Refresh the settings display
            RefreshSettings(_pipeline);
        }

        /// <summary>
        /// Remove a key-value entry from ArgumentsMap
        /// </summary>
        private void RemoveArgumentEntry(SerializedProperty fieldProp, int index)
        {
            var argumentsProp = fieldProp.FindPropertyRelative("arguments");
            if (argumentsProp == null)
                return;

            var keysProp = argumentsProp.FindPropertyRelative("keys");
            var valuesProp = argumentsProp.FindPropertyRelative("values");

            if (keysProp == null || valuesProp == null)
                return;

            _serializedObject.Update();

            if (index >= 0 && index < keysProp.arraySize)
            {
                keysProp.DeleteArrayElementAtIndex(index);
                valuesProp.DeleteArrayElementAtIndex(index);
            }

            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_pipeline);
            AssetDatabase.SaveAssets();

            // Refresh the settings display
            RefreshSettings(_pipeline);
        }

        /// <summary>
        /// Refresh visibility of fields that depend on ShowIf conditions
        /// </summary>
        private void RefreshDependentFields()
        {
            if (_pipeline == null || _serializedObject == null)
                return;

            // Update serialized object from current state
            _serializedObject.Update();
            
            var buildDataType = typeof(UniBuildConfigurationData);
            var fields = buildDataType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (!_fieldElements.ContainsKey(field.Name))
                    continue;

                var element = _fieldElements[field.Name];
                if (element == null)
                    continue;

                bool shouldShow = ShouldShowField(field);
                var currentDisplay = element.style.display;
                var newDisplay = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
                
                if (currentDisplay != newDisplay)
                {
                    element.style.display = newDisplay;
                }
            }
        }

        /// <summary>
        /// Get a nice label for a field
        /// </summary>
        private string GetFieldLabel(FieldInfo field)
        {
            // For toggle fields, use nicified name (shorter)
            // For other fields, try to get Tooltip first
            if (field.FieldType == typeof(bool))
            {
                return ObjectNames.NicifyVariableName(field.Name);
            }

            // For non-bool fields, try to get custom label from Tooltip attribute
            var tooltipAttr = field.GetCustomAttribute<TooltipAttribute>();
            if (tooltipAttr != null && !string.IsNullOrEmpty(tooltipAttr.tooltip))
                return tooltipAttr.tooltip;

            // Convert field name to nice label (CamelCase to Title Case)
            return ObjectNames.NicifyVariableName(field.Name);
        }

        private void CreatePlayerBuildSection(ScrollView scrollView)
        {
            var section = CreateSection("Build Execution", scrollView);

            var playerBuildToggle = new Toggle("Enable Player Build");
            playerBuildToggle.value = _pipeline.PlayerBuildEnabled;
            playerBuildToggle.RegisterValueChangedCallback(evt =>
            {
                _serializedObject.Update();
                _serializedObject.FindProperty("playerBuildEnabled").boolValue = evt.newValue;
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_pipeline);
            });
            section.Add(playerBuildToggle);

            var infoLabel = new Label("When disabled, only pre-build and post-build commands will execute");
            infoLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            infoLabel.style.fontSize = 10;
            infoLabel.style.marginTop = 2;
            infoLabel.style.marginBottom = 2;
            section.Add(infoLabel);

            // Add Apply Settings button
            var applyButton = new Button(() => ApplyProjectSettings()) { text = "Apply Settings to Project" };
            applyButton.style.marginTop = 6;
            applyButton.style.paddingTop = 4;
            applyButton.style.paddingBottom = 4;
            applyButton.style.fontSize = 12;
            applyButton.style.backgroundColor = new StyleColor(new Color(0.2f, 0.5f, 0.2f));
            section.Add(applyButton);
        }

        /// <summary>
        /// Apply pipeline settings to the Unity project
        /// </summary>
        private void ApplyProjectSettings()
        {
            if (_pipeline == null)
            {
                Debug.LogWarning("No pipeline selected");
                return;
            }

            try
            {
                _pipeline.ApplySettings();
                Debug.Log($"Applied settings from pipeline: {_pipeline.name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to apply pipeline settings: {ex.Message}");
            }
        }

        private VisualElement CreateSection(string title, ScrollView parent)
        {
            var section = new VisualElement();
            section.style.marginBottom = 8;
            section.style.paddingLeft = 8;
            section.style.paddingRight = 8;
            section.style.paddingTop = 4;
            section.style.paddingBottom = 4;
            section.style.borderTopWidth = 1;
            section.style.borderBottomWidth = 1;
            section.style.borderLeftWidth = 1;
            section.style.borderRightWidth = 1;
            var borderColor = new Color(0.3f, 0.3f, 0.3f);
            section.style.borderTopColor = new StyleColor(borderColor);
            section.style.borderBottomColor = new StyleColor(borderColor);
            section.style.borderLeftColor = new StyleColor(borderColor);
            section.style.borderRightColor = new StyleColor(borderColor);

            var titleLabel = new Label(title);
            titleLabel.style.fontSize = 14;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 6;
            section.Add(titleLabel);

            var contentContainer = new VisualElement();
            contentContainer.style.flexDirection = FlexDirection.Column;
            section.Add(contentContainer);

            parent.Add(section);

            return contentContainer;
        }
    }
}
