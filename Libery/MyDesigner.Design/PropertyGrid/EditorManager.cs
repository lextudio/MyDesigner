// Copyright (c) 2019 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace MyDesigner.Design.PropertyGrid;

/// <summary>
///     Manages registered type and property editors.
/// </summary>
public static class EditorManager
{
    // property return type => editor type
    private static readonly Dictionary<Type, Type> typeEditors = new();

    // property full name => editor type
    private static readonly Dictionary<string, Type> propertyEditors = new();

    private static Type defaultComboboxEditor;

    private static Type defaultTextboxEditor;

    /// <summary>
    ///     Creates a property editor for the specified <paramref name="property" />
    /// </summary>
    public static Control CreateEditor(DesignItemProperty property)
    {
        // التحقق من أن الخاصية هي Event
        if (property.IsEvent)
        {
            // إنشاء EventEditor
            try
            {
                var eventEditorType = Type.GetType("MyDesigner.Designer.PropertyGrid.Editors.EventEditor, MyDesigner.Designer");
                if (eventEditorType != null)
                {
                    return (Control)Activator.CreateInstance(eventEditorType);
                }
            }
            catch
            {
                // إذا فشل، استخدم TextBox عادي
            }
        }

        Type editorType;
        if (!propertyEditors.TryGetValue(property.FullName, out editorType))
        {
            var type = property.ReturnType;
            while (type != null)
            {
                if (typeEditors.TryGetValue(type, out editorType)) break;
                type = type.BaseType;
            }

            foreach (var t in typeEditors)
                if (t.Key.IsAssignableFrom(property.ReturnType))
                    return (Control)Activator.CreateInstance(t.Value);

            if (editorType == null)
            {
                IEnumerable standardValues = null;

                if (property.AvaloniaProperty != null)
                    standardValues = Metadata.GetStandardValues(property.AvaloniaProperty);
                if (standardValues == null) standardValues = Metadata.GetStandardValues(property.ReturnType);

                if (standardValues != null)
                {
                    var itemsControl = (ItemsControl)Activator.CreateInstance(defaultComboboxEditor);
                    itemsControl.ItemsSource = standardValues;
                    if (Nullable.GetUnderlyingType(property.ReturnType) != null)
                        itemsControl.GetType().GetProperty("IsNullable")
                            ?.SetValue(itemsControl, true, null); //In this Class we don't know the Nullable Combo Box
                    return itemsControl;
                }

                var namedStandardValues = Metadata.GetNamedStandardValues(property.ReturnType);
                if (namedStandardValues != null)
                {
                    var itemsControl = (ItemsControl)Activator.CreateInstance(defaultComboboxEditor);
                    itemsControl.ItemsSource = namedStandardValues;
                    itemsControl.DisplayMemberBinding = new Binding("Name");
                    if (itemsControl is SelectingItemsControl selector)
                        selector.SelectedValueBinding = new Binding("Value");
                    if (Nullable.GetUnderlyingType(property.ReturnType) != null)
                        itemsControl.GetType().GetProperty("IsNullable")
                            ?.SetValue(itemsControl, true, null); //In this Class we don't know the Nullable Combo Box
                    return itemsControl;
                }

                return (Control)Activator.CreateInstance(defaultTextboxEditor);
            }
        }

        return (Control)Activator.CreateInstance(editorType);
    }

    /// <summary>
    ///     Registers the Textbox Editor.
    /// </summary>
    public static void SetDefaultTextBoxEditorType(Type type)
    {
        defaultTextboxEditor = type;
    }

    /// <summary>
    ///     Registers the Combobox Editor.
    /// </summary>
    public static void SetDefaultComboBoxEditorType(Type type)
    {
        defaultComboboxEditor = type;
    }

    /// <summary>
    ///     Registers property editors defined in the specified assembly.
    /// </summary>
    public static void RegisterAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException("assembly");

        foreach (var type in assembly.GetExportedTypes())
        {
            foreach (TypeEditorAttribute editorAttribute in
                     type.GetCustomAttributes(typeof(TypeEditorAttribute), false))
            {
                CheckValidEditor(type);
                typeEditors[editorAttribute.SupportedPropertyType] = type;
            }

            foreach (PropertyEditorAttribute editorAttribute in type.GetCustomAttributes(
                         typeof(PropertyEditorAttribute), false))
            {
                CheckValidEditor(type);
                var propertyName = editorAttribute.PropertyDeclaringType.FullName + "." + editorAttribute.PropertyName;
                propertyEditors[propertyName] = type;
            }
        }
    }

    private static void CheckValidEditor(Type type)
    {
        if (!typeof(Control).IsAssignableFrom(type))
            throw new DesignerException("Editor types must derive from Control!");
    }
}