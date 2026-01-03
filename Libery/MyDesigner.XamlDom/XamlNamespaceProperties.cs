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

using Avalonia;
using Avalonia.Controls;

namespace MyDesigner.XamlDom;

/// <summary>
///     Helper Class for the Markup Compatibility Properties used by VS and Blend
/// </summary>
public class XamlNamespaceProperties : Control
{
    #region Class

    /// <summary>
    ///     Getter for the <see cref="ClassProperty" />
    /// </summary>
    public static string GetClass(AvaloniaObject obj)
    {
        return (string)obj.GetValue(ClassProperty);
    }

    /// <summary>
    ///     Setter for the <see cref="ClassProperty" />
    /// </summary>
    public static void SetClass(AvaloniaObject obj, string value)
    {
        obj.SetValue(ClassProperty, value);
    }

    /// <summary>
    ///     Class-Name Property
    /// </summary>
    public static readonly AttachedProperty<string> ClassProperty =
        AvaloniaProperty.RegisterAttached<XamlNamespaceProperties, AvaloniaObject, string>("Class");

    #endregion


    #region ClassModifier

    /// <summary>
    ///     Getter for the <see cref="ClassModifierProperty" />
    /// </summary>
    public static string GetClassModifier(AvaloniaObject obj)
    {
        return (string)obj.GetValue(ClassModifierProperty);
    }

    /// <summary>
    ///     Setter for the <see cref="ClassModifierProperty" />
    /// </summary>
    public static void SetClassModifier(AvaloniaObject obj, string value)
    {
        obj.SetValue(ClassModifierProperty, value);
    }

    /// <summary>
    ///     Class Modifier Property
    /// </summary>
    public static readonly AttachedProperty<string> ClassModifierProperty =
        AvaloniaProperty.RegisterAttached<XamlNamespaceProperties, AvaloniaObject, string>("ClassModifier");

    #endregion

    #region TypeArguments

    /// <summary>
    ///     Getter for the <see cref="TypeArgumentsProperty" />
    /// </summary>
    public static string GetTypeArguments(AvaloniaObject obj)
    {
        return (string)obj.GetValue(TypeArgumentsProperty);
    }

    /// <summary>
    ///     Getter for the <see cref="TypeArgumentsProperty" />
    /// </summary>
    public static void SetTypeArguments(AvaloniaObject obj, string value)
    {
        obj.SetValue(TypeArgumentsProperty, value);
    }

    /// <summary>
    ///     Type Arguments Property
    /// </summary>
    public static readonly AttachedProperty<string> TypeArgumentsProperty =
        AvaloniaProperty.RegisterAttached<XamlNamespaceProperties, AvaloniaObject, string>("TypeArguments");

    #endregion
}