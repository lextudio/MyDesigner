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

namespace MyDesigner.Design;

/// <summary>
///     Extension methods used in the Avalonia designer.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    ///     Rounds position and size of a Rect to PlacementInformation.BoundsPrecision digits.
    /// </summary>
    public static Rect Round(this Rect rect)
    {
        return new Rect(
            Math.Round(rect.X, PlacementInformation.BoundsPrecision),
            Math.Round(rect.Y, PlacementInformation.BoundsPrecision),
            Math.Round(rect.Width, PlacementInformation.BoundsPrecision),
            Math.Round(rect.Height, PlacementInformation.BoundsPrecision)
        );
    }

    /// <summary>
    ///     Gets the design item property for the specified member descriptor.
    /// </summary>
    public static DesignItemProperty GetProperty(this DesignItemPropertyCollection properties, MemberDescriptor md)
    {
        DesignItemProperty prop = null;

        var pd = md as PropertyDescriptor;
        if (pd != null)
        {
            // Note: Avalonia uses AvaloniaProperty instead of DependencyProperty
            // This will need to be adapted based on the actual Avalonia property system
            // For now, we'll use the property name approach
        }

        if (prop == null) prop = properties[md.Name];

        return prop;
    }

    /// <summary>
    ///     Gets if the specified design item property represents an attached property.
    /// </summary>
    public static bool IsAttachedProperty(this DesignItemProperty property)
    {
        // Note: This will need to be adapted for Avalonia's attached property system
        // Avalonia uses AvaloniaProperty.IsAttached
        return false; // Placeholder implementation
    }

    /// <summary>
    ///     Gets if the specified design item property represents an attached dependency property.
    ///     In Avalonia, this is equivalent to IsAttachedProperty.
    /// </summary>
    public static bool IsAttachedDependencyProperty(this DesignItemProperty property)
    {
        if (property.AvaloniaProperty != null)
        {
            // In Avalonia, we can check if the property is attached
            return property.AvaloniaProperty.IsAttached;
        }

        return false;
    }
}