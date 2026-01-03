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
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace MyDesigner.Design.UIExtensions;

/// <summary>
///     Contains helper methods for UI.
/// </summary>
public static class UIHelpers
{
    /// <summary>
    ///     Gets the parent. Which tree the parent is retrieved from depends on the parameters.
    /// </summary>
    /// <param name="child">The child to get parent for.</param>
    /// <param name="searchCompleteVisualTree">
    ///     If true the parent in the visual tree is returned, if false the parent may be
    ///     retrieved from the logical tree.
    /// </param>
    /// <returns>
    ///     The parent element, retrieved from either visual tree or logical tree.
    /// </returns>
    public static StyledElement GetParentObject(this StyledElement child, bool searchCompleteVisualTree)
    {
        if (child == null) return null;

        if (!searchCompleteVisualTree)
        {
            // Try logical parent first
            var logicalParent = child.GetLogicalParent<StyledElement>();
            if (logicalParent != null) return logicalParent;
        }

        // Fall back to visual parent
        if (child is Visual visual)
            return visual.GetVisualParent<StyledElement>();
        
        return null;
    }

    /// <summary>
    ///     Gets first parent element of the specified type. Which tree the parent is retrieved from depends on the parameters.
    /// </summary>
    /// <param name="child">The child to get parent for.</param>
    /// <param name="searchCompleteVisualTree">
    ///     If true the parent in the visual tree is returned, if false the parent may be
    ///     retrieved from the logical tree.
    /// </param>
    /// <returns>
    ///     The first parent element of the specified type, retrieved from either visual tree or logical tree.
    ///     null is returned if no parent of the specified type is found.
    /// </returns>
    public static T TryFindParent<T>(this StyledElement child, bool searchCompleteVisualTree = false)
        where T : StyledElement
    {
        var parentObject = GetParentObject(child, searchCompleteVisualTree);

        if (parentObject == null) return null;

        var parent = parentObject as T;
        if (parent != null) return parent;

        return TryFindParent<T>(parentObject, searchCompleteVisualTree);
    }

    /// <summary>
    ///     Returns the first child of the specified type found in the visual tree.
    /// </summary>
    /// <param name="parent">The parent element where the search is started.</param>
    /// <returns>
    ///     The first child of the specified type found in the visual tree, or null if no parent of the specified type is
    ///     found.
    /// </returns>
    public static T TryFindChild<T>(this StyledElement parent) where T : StyledElement
    {
        if (parent is Visual visual)
        {
            foreach (var child in visual.GetVisualChildren())
            {
                if (child is T typedChild) return typedChild;
                
                if (child is StyledElement styledChild)
                {
                    var foundChild = TryFindChild<T>(styledChild);
                    if (foundChild != null) return foundChild;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Returns the first child of the specified type and with the specified name found in the visual tree.
    /// </summary>
    /// <param name="parent">The parent element where the search is started.</param>
    /// <param name="childName">The name of the child element to find, or an empty string or null to only look at the type.</param>
    /// <returns>The first child that matches the specified type and child name, or null if no match is found.</returns>
    public static T TryFindChild<T>(this StyledElement parent, string childName) where T : StyledElement
    {
        if (parent == null) return null;
        
        if (parent is Visual visual)
        {
            foreach (var child in visual.GetVisualChildren())
            {
                if (child is T typedChild)
                {
                    if (string.IsNullOrEmpty(childName))
                    {
                        return typedChild;
                    }
                    else if (child is Control control && control.Name == childName)
                    {
                        return typedChild;
                    }
                }
                
                if (child is StyledElement styledChild)
                {
                    var foundChild = TryFindChild<T>(styledChild, childName);
                    if (foundChild != null) return foundChild;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Returns the first ancestor of specified type
    /// </summary>
    public static T FindAncestor<T>(StyledElement current) where T : StyledElement
    {
        current = GetVisualOrLogicalParent(current);

        while (current != null)
        {
            if (current is T ancestor) return ancestor;
            current = GetVisualOrLogicalParent(current);
        }

        return null;
    }

    private static StyledElement GetVisualOrLogicalParent(StyledElement obj)
    {
        // Try visual parent first
        if (obj is Visual visual)
        {
            var visualParent = visual.GetVisualParent<StyledElement>();
            if (visualParent != null) return visualParent;
        }
        
        // Fall back to logical parent
        return obj.GetLogicalParent<StyledElement>();
    }
}