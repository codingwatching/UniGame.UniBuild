namespace UniGame.UniBuild.Editor.Inspector.Editors
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Factory for creating property editors for various field types
    /// Consolidates all field editing logic and eliminates duplication
    /// </summary>
    public static class PropertyEditorFactory
    {
        private static UniBuildPipeline _selectedPipeline; // Reference for SetDirty

        public static void SetPipelineReference(UniBuildPipeline pipeline)
        {
            _selectedPipeline = pipeline;
        }

        /// <summary>
        /// Create an editor for a field based on its type
        /// </summary>
        public static VisualElement CreateFieldEditor(FieldInfo fieldInfo, object instance)
        {
            var fieldValue = fieldInfo.GetValue(instance);
            var fieldType = fieldInfo.FieldType;

            // Handle basic types
            if (fieldType == typeof(string))
                return CreateStringEditor(fieldInfo, instance, (string)fieldValue);
            
            if (fieldType == typeof(int))
                return CreateIntEditor(fieldInfo, instance, (int)fieldValue);
            
            if (fieldType == typeof(float))
                return CreateFloatEditor(fieldInfo, instance, (float)fieldValue);
            
            if (fieldType == typeof(bool))
                return CreateBoolEditor(fieldInfo, instance, (bool)fieldValue);
            
            if (fieldType == typeof(Vector2))
                return CreateVector2Editor(fieldInfo, instance, (Vector2)fieldValue);
            
            if (fieldType == typeof(Vector3))
                return CreateVector3Editor(fieldInfo, instance, (Vector3)fieldValue);
            
            if (fieldType == typeof(Vector4))
                return CreateVector4Editor(fieldInfo, instance, (Vector4)fieldValue);
            
            if (fieldType == typeof(Color))
                return CreateColorEditor(fieldInfo, instance, (Color)fieldValue);
            
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
                return CreateObjectEditor(fieldInfo, instance, fieldValue as UnityEngine.Object, fieldType);
            
            if (fieldType.IsEnum)
                return CreateEnumEditor(fieldInfo, instance, (Enum)fieldValue);
            
            if (IsListType(fieldType))
                return CreateListEditor(fieldInfo, instance, fieldValue as IList);
            
            // Fallback: read-only label
            return CreateReadOnlyEditor(fieldValue);
        }

        /// <summary>
        /// Create string editor
        /// </summary>
        private static VisualElement CreateStringEditor(FieldInfo fieldInfo, object instance, string value)
        {
            var textField = new TextField();
            textField.value = value ?? "";
            textField.style.flexGrow = 1;
            ApplyCompactStyle(textField);
            textField.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return textField;
        }

        /// <summary>
        /// Create int editor
        /// </summary>
        private static VisualElement CreateIntEditor(FieldInfo fieldInfo, object instance, int value)
        {
            var intField = new IntegerField();
            intField.value = value;
            intField.style.flexGrow = 1;
            ApplyCompactStyle(intField);
            intField.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return intField;
        }

        /// <summary>
        /// Create float editor
        /// </summary>
        private static VisualElement CreateFloatEditor(FieldInfo fieldInfo, object instance, float value)
        {
            var floatField = new FloatField();
            floatField.value = value;
            floatField.style.flexGrow = 1;
            ApplyCompactStyle(floatField);
            floatField.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return floatField;
        }

        /// <summary>
        /// Create bool editor
        /// </summary>
        private static VisualElement CreateBoolEditor(FieldInfo fieldInfo, object instance, bool value)
        {
            var boolField = new Toggle();
            boolField.value = value;
            boolField.style.flexGrow = 1;
            ApplyCompactStyle(boolField);
            boolField.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return boolField;
        }

        /// <summary>
        /// Create Vector2 editor
        /// </summary>
        private static VisualElement CreateVector2Editor(FieldInfo fieldInfo, object instance, Vector2 value)
        {
            var v2Field = new Vector2Field();
            v2Field.value = value;
            v2Field.style.flexGrow = 1;
            ApplyCompactStyle(v2Field);
            v2Field.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return v2Field;
        }

        /// <summary>
        /// Create Vector3 editor
        /// </summary>
        private static VisualElement CreateVector3Editor(FieldInfo fieldInfo, object instance, Vector3 value)
        {
            var v3Field = new Vector3Field();
            v3Field.value = value;
            v3Field.style.flexGrow = 1;
            ApplyCompactStyle(v3Field);
            v3Field.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return v3Field;
        }

        /// <summary>
        /// Create Vector4 editor
        /// </summary>
        private static VisualElement CreateVector4Editor(FieldInfo fieldInfo, object instance, Vector4 value)
        {
            var v4Field = new Vector4Field();
            v4Field.value = value;
            v4Field.style.flexGrow = 1;
            ApplyCompactStyle(v4Field);
            v4Field.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return v4Field;
        }

        /// <summary>
        /// Create Color editor
        /// </summary>
        private static VisualElement CreateColorEditor(FieldInfo fieldInfo, object instance, Color value)
        {
            var colorField = new ColorField();
            colorField.value = value;
            colorField.style.flexGrow = 1;
            ApplyCompactStyle(colorField);
            colorField.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return colorField;
        }

        /// <summary>
        /// Create Object reference editor
        /// </summary>
        private static VisualElement CreateObjectEditor(FieldInfo fieldInfo, object instance, UnityEngine.Object value, Type objectType)
        {
            var objField = new ObjectField();
            objField.objectType = objectType;
            objField.value = value;
            objField.style.flexGrow = 1;
            ApplyCompactStyle(objField);
            objField.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return objField;
        }

        /// <summary>
        /// Create Enum editor
        /// </summary>
        private static VisualElement CreateEnumEditor(FieldInfo fieldInfo, object instance, Enum value)
        {
            var enumField = new EnumField(value);
            enumField.style.flexGrow = 1;
            ApplyCompactStyle(enumField);
            enumField.RegisterValueChangedCallback(evt =>
            {
                fieldInfo.SetValue(instance, evt.newValue);
                MarkDirty();
            });
            return enumField;
        }

        /// <summary>
        /// Create List editor with foldout
        /// </summary>
        private static VisualElement CreateListEditor(FieldInfo fieldInfo, object instance, IList listInstance)
        {
            if (listInstance == null || listInstance.Count == 0)
                return new VisualElement(); // Return empty container for empty lists

            var container = new VisualElement();
            var listFoldout = new Foldout { text = $"{fieldInfo.Name} ({listInstance.Count} items)", value = false };
            listFoldout.style.fontSize = UIThemeConstants.FontSizes.Normal;
            listFoldout.style.marginLeft = UIThemeConstants.Spacing.Padding;
            listFoldout.style.marginBottom = 4;

            var listItemsContainer = UIElementFactory.CreateListItemsContainer();

            for (int i = 0; i < listInstance.Count; i++)
            {
                var item = listInstance[i];
                var itemContainer = CreateListItemEditor(fieldInfo, item, i, listInstance);
                listItemsContainer.Add(itemContainer);
            }

            listFoldout.Add(listItemsContainer);
            container.Add(listFoldout);
            return container;
        }

        /// <summary>
        /// Create editor for a single list item
        /// </summary>
        private static VisualElement CreateListItemEditor(FieldInfo fieldInfo, object item, int index, IList listInstance)
        {
            var itemContainer = new VisualElement();
            itemContainer.style.flexDirection = FlexDirection.Row;
            itemContainer.style.alignItems = Align.Center;
            itemContainer.style.justifyContent = Justify.SpaceBetween;
            itemContainer.style.paddingLeft = UIThemeConstants.Spacing.SmallPadding;
            itemContainer.style.paddingRight = UIThemeConstants.Spacing.SmallPadding;
            itemContainer.style.paddingTop = 1;
            itemContainer.style.paddingBottom = 1;
            itemContainer.style.marginBottom = 2;
            itemContainer.style.borderBottomWidth = UIThemeConstants.Sizes.BorderWidth;
            itemContainer.style.borderBottomColor = new StyleColor(UIThemeConstants.Colors.BorderDark);

            var itemIndexLabel = new Label($"[{index}]");
            itemIndexLabel.style.fontSize = UIThemeConstants.FontSizes.Small;
            itemIndexLabel.style.minWidth = 30;
            itemIndexLabel.style.color = new StyleColor(UIThemeConstants.Colors.TextDimmed);
            itemContainer.Add(itemIndexLabel);

            int indexCopy = index;
            VisualElement itemEditor = null;

            if (item == null)
            {
                itemEditor = UIElementFactory.CreateDimmedLabel("null", UIThemeConstants.FontSizes.Small);
            }
            else if (item is string stringItem)
            {
                var stringField = new TextField();
                stringField.value = stringItem;
                stringField.style.flexGrow = 1;
                stringField.RegisterValueChangedCallback(evt =>
                {
                    listInstance[indexCopy] = evt.newValue;
                    MarkDirty();
                });
                itemEditor = stringField;
            }
            else if (item is int intItem)
            {
                var intField = new IntegerField();
                intField.value = intItem;
                intField.style.flexGrow = 1;
                intField.RegisterValueChangedCallback(evt =>
                {
                    listInstance[indexCopy] = evt.newValue;
                    MarkDirty();
                });
                itemEditor = intField;
            }
            else if (item is float floatItem)
            {
                var floatField = new FloatField();
                floatField.value = floatItem;
                floatField.style.flexGrow = 1;
                floatField.RegisterValueChangedCallback(evt =>
                {
                    listInstance[indexCopy] = evt.newValue;
                    MarkDirty();
                });
                itemEditor = floatField;
            }
            else if (item is bool boolItem)
            {
                var boolField = new Toggle();
                boolField.value = boolItem;
                boolField.style.flexGrow = 1;
                boolField.RegisterValueChangedCallback(evt =>
                {
                    listInstance[indexCopy] = evt.newValue;
                    MarkDirty();
                });
                itemEditor = boolField;
            }
            else
            {
                itemEditor = UIElementFactory.CreateDimmedLabel(item.ToString(), UIThemeConstants.FontSizes.Small);
            }

            if (itemEditor != null)
            {
                itemEditor.style.minWidth = UIThemeConstants.Sizes.EditorMinWidth;
                itemContainer.Add(itemEditor);
            }

            return itemContainer;
        }

        /// <summary>
        /// Create read-only editor for unsupported types
        /// </summary>
        private static VisualElement CreateReadOnlyEditor(object fieldValue)
        {
            var valueLabel = new Label(fieldValue?.ToString() ?? "null");
            valueLabel.style.fontSize = UIThemeConstants.FontSizes.Small;
            valueLabel.style.color = new StyleColor(UIThemeConstants.Colors.TextVeryDimmed);
            valueLabel.style.flexGrow = 1;
            return valueLabel;
        }

        /// <summary>
        /// Check if type is List<T>
        /// </summary>
        private static bool IsListType(Type type)
        {
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                return genericDef == typeof(List<>);
            }
            return false;
        }

        /// <summary>
        /// Mark pipeline as dirty
        /// </summary>
        private static void MarkDirty()
        {
            if (_selectedPipeline != null)
                EditorUtility.SetDirty(_selectedPipeline);
        }

        /// <summary>
        /// Apply compact vertical spacing to an element
        /// </summary>
        private static void ApplyCompactStyle(VisualElement element)
        {
            element.style.marginTop = 0;
            element.style.marginBottom = 0;
            element.style.paddingTop = 2;
            element.style.paddingBottom = 2;
        }
    }
}
