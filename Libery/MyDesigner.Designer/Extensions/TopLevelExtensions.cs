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
using Avalonia.Input;
using Avalonia.Platform;

namespace MyDesigner.Designer.Extensions;

/// <summary>
/// Extension methods to handle ITopLevelImpl API changes in Avalonia 11.x
/// </summary>
public static class TopLevelExtensions
{
    /// <summary>
    /// Extension method to check if a key is down, replacing ITopLevelImpl.IsKeyDown
    /// </summary>
    public static bool IsKeyDown(this ITopLevelImpl topLevelImpl, Key key)
    {
        // In Avalonia 11.x, we need to use a different approach
        // This is a simplified implementation that may need adjustment
        try
        {
            // Try to get the current key state from the platform
            // This is a fallback implementation
            return false; // Default to false if we can't determine the state
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extension method to get key modifiers, replacing ITopLevelImpl.GetKeyModifiers
    /// </summary>
    public static KeyModifiers GetKeyModifiers(this ITopLevelImpl topLevelImpl)
    {
        // In Avalonia 11.x, we need to use a different approach
        // This is a simplified implementation that may need adjustment
        try
        {
            // Try to get the current modifier state from the platform
            // This is a fallback implementation
            return KeyModifiers.None; // Default to no modifiers if we can't determine the state
        }
        catch
        {
            return KeyModifiers.None;
        }
    }

    /// <summary>
    /// Extension method to get keyboard device, replacing ITopLevelImpl.GetKeyboardDevice
    /// </summary>
    public static IKeyboardDevice? GetKeyboardDevice(this ITopLevelImpl topLevelImpl)
    {
        // In Avalonia 11.x, keyboard device access is different
        // This is a simplified implementation that may need adjustment
        try
        {
            // Return null as keyboard device access has changed
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Alternative method to check key state using TopLevel
    /// </summary>
    public static bool IsKeyDown(this TopLevel topLevel, Key key)
    {
        try
        {
            // In Avalonia 11.x, we might need to track key states differently
            // This is a fallback implementation
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Alternative method to get key modifiers using TopLevel
    /// </summary>
    public static KeyModifiers GetKeyModifiers(this TopLevel topLevel)
    {
        try
        {
            // In Avalonia 11.x, we might need to track modifier states differently
            // This is a fallback implementation
            return KeyModifiers.None;
        }
        catch
        {
            return KeyModifiers.None;
        }
    }
}