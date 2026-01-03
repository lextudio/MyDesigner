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

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;

namespace MyDesigner.XamlDom;

/// <summary>
///     Helper Class for the Design Time Properties used by VS and Blend
/// </summary>
public static class DesignTimeProperties
{
    // Dummy class to use as type argument since static classes can't be used as type arguments
    private class DesignTimePropertiesOwner { }

    #region IsHidden

    /// <summary>
    ///     Getter for <see cref="IsHiddenProperty" />
    /// </summary>
    public static bool GetIsHidden(AvaloniaObject obj)
    {
        return obj.GetValue(IsHiddenProperty);
    }

    /// <summary>
    ///     Setter for <see cref="IsHiddenProperty" />
    /// </summary>
    public static void SetIsHidden(AvaloniaObject obj, bool value)
    {
        obj.SetValue(IsHiddenProperty, value);
    }

    /// <summary>
    ///     Design-time IsHidden property
    /// </summary>
    public static readonly AttachedProperty<bool> IsHiddenProperty =
        AvaloniaProperty.RegisterAttached<DesignTimePropertiesOwner, AvaloniaObject, bool>("IsHidden");

    #endregion

    //#region IsLocked

    ///// <summary>
    /////     Getter for <see cref="IsLockedProperty" />
    ///// </summary>
    //public static bool GetIsLocked(AvaloniaObject obj)
    //{
    //    return obj.GetValue(IsLockedProperty);
    //}

    ///// <summary>
    /////     Setter for <see cref="IsLockedProperty" />
    ///// </summary>
    //public static void SetIsLocked(AvaloniaObject obj, bool value)
    //{
    //    obj.SetValue(IsLockedProperty, value);
    //}

    ///// <summary>
    /////     Design-time IsLocked property.
    ///// </summary>
    //public static readonly AttachedProperty<bool> IsLockedProperty =
    //    AvaloniaProperty.RegisterAttached<DesignTimePropertiesOwner, AvaloniaObject, bool>("IsLocked");

    //#endregion

    #region DataContext

    /// <summary>
    ///     Getter for <see cref="DataContextProperty" />
    /// </summary>
    public static object GetDataContext(AvaloniaObject obj)
    {
        return obj.GetValue(DataContextProperty);
    }

    /// <summary>
    ///     Setter for <see cref="DataContextProperty" />
    /// </summary>
    public static void SetDataContext(AvaloniaObject obj, object value)
    {
        obj.SetValue(DataContextProperty, value);
    }

    /// <summary>
    ///     Design-time data context
    /// </summary>
    public static readonly AttachedProperty<object> DataContextProperty =
        AvaloniaProperty.RegisterAttached<DesignTimePropertiesOwner, AvaloniaObject, object>("DataContext");

    #endregion

    #region DesignSource

    /// <summary>
    ///     Getter for <see cref="DesignSourceProperty" />
    /// </summary>
    public static object GetDesignSource(AvaloniaObject obj)
    {
        return obj.GetValue(DesignSourceProperty);
    }

    /// <summary>
    ///     Setter for <see cref="DesignSourceProperty" />
    /// </summary>
    public static void SetDesignSource(AvaloniaObject obj, object value)
    {
        obj.SetValue(DesignSourceProperty, value);
    }

    /// <summary>
    ///     Design-time design source
    /// </summary>
    public static readonly AttachedProperty<object> DesignSourceProperty =
        AvaloniaProperty.RegisterAttached<DesignTimePropertiesOwner, AvaloniaObject, object>("DesignSource");

    #endregion

    #region DesignWidth

    /// <summary>
    ///     Getter for <see cref="DesignWidthProperty" />
    /// </summary>
    public static double GetDesignWidth(AvaloniaObject obj)
    {
        return obj.GetValue(DesignWidthProperty);
    }

    /// <summary>
    ///     Setter for <see cref="DesignWidthProperty" />
    /// </summary>
    public static void SetDesignWidth(AvaloniaObject obj, double value)
    {
        obj.SetValue(DesignWidthProperty, value);
    }

    /// <summary>
    ///     Design-time width
    /// </summary>
    public static readonly AttachedProperty<double> DesignWidthProperty =
        AvaloniaProperty.RegisterAttached<DesignTimePropertiesOwner, AvaloniaObject, double>("DesignWidth");

    #endregion

    #region DesignHeight

    /// <summary>
    ///     Getter for <see cref="DesignHeightProperty" />
    /// </summary>
    public static double GetDesignHeight(AvaloniaObject obj)
    {
        return obj.GetValue(DesignHeightProperty);
    }

    /// <summary>
    ///     Setter for <see cref="DesignHeightProperty" />
    /// </summary>
    public static void SetDesignHeight(AvaloniaObject obj, double value)
    {
        obj.SetValue(DesignHeightProperty, value);
    }

    /// <summary>
    ///     Design-time height
    /// </summary>
    public static readonly AttachedProperty<double> DesignHeightProperty =
        AvaloniaProperty.RegisterAttached<DesignTimePropertiesOwner, AvaloniaObject, double>("DesignHeight");

    #endregion

    #region LayoutOverrides

    /// <summary>
    ///     Getter for <see cref="LayoutOverridesProperty" />
    /// </summary>
    public static string GetLayoutOverrides(AvaloniaObject obj)
    {
        return obj.GetValue(LayoutOverridesProperty);
    }

    /// <summary>
    ///     Setter for <see cref="LayoutOverridesProperty" />
    /// </summary>
    public static void SetLayoutOverrides(AvaloniaObject obj, string value)
    {
        obj.SetValue(LayoutOverridesProperty, value);
    }

    /// <summary>
    ///     Layout-Overrides
    /// </summary>
    public static readonly AttachedProperty<string> LayoutOverridesProperty =
        AvaloniaProperty.RegisterAttached<DesignTimePropertiesOwner, AvaloniaObject, string>("LayoutOverrides");

    #endregion

    #region LayoutRounding

    /// <summary>
    ///     Getter for <see cref="LayoutRoundingProperty" />
    /// </summary>
    public static bool GetLayoutRounding(AvaloniaObject obj)
    {
        return obj.GetValue(LayoutRoundingProperty);
    }

    /// <summary>
    ///     Setter for <see cref="LayoutRoundingProperty" />
    /// </summary>
    public static void SetLayoutRounding(AvaloniaObject obj, bool value)
    {
        obj.SetValue(LayoutRoundingProperty, value);
    }

    /// <summary>
    ///     Design-time layout rounding
    /// </summary>
    public static readonly AttachedProperty<bool> LayoutRoundingProperty =
        AvaloniaProperty.RegisterAttached<DesignTimePropertiesOwner, AvaloniaObject, bool>("LayoutRounding");

    #endregion
}