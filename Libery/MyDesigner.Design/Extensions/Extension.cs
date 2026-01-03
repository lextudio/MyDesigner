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

namespace MyDesigner.Design.Extensions;

/// <summary>
///     Base class for extensions.
/// </summary>
public abstract class Extension
{
    /// <summary>
    ///     Gets the extended design item.
    /// </summary>
    public DesignItem ExtendedItem { get; internal set; }

    /// <summary>
    ///     Sets the extended design item.
    /// </summary>
    internal void SetExtendedItem(DesignItem item)
    {
        ExtendedItem = item;
    }

    /// <summary>
    ///     Gets the disabled extensions for a visual.
    /// </summary>
    public static string GetDisabledExtensions(Visual visual)
    {
        // Placeholder implementation for Avalonia
        return string.Empty;
    }

    /// <summary>
    ///     Gets whether mouse over extensions are disabled for a visual.
    /// </summary>
    public static bool GetDisableMouseOverExtensions(Visual visual)
    {
        // Placeholder implementation for Avalonia
        return false;
    }
}