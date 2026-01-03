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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;

namespace MyDesigner.XamlDom;

/// <summary>
///     Static methods to help with <see cref="INameScope" /> operations on Xaml elements.
/// </summary>
public static class NameScopeHelper
{
    /// <summary>
    ///     Finds the XAML namescope for the specified object and uses it to unregister the old name and then register the new
    ///     name.
    /// </summary>
    /// <param name="namedObject">The object where the name was changed.</param>
    /// <param name="oldName">The old name.</param>
    /// <param name="newName">The new name.</param>
    public static void NameChanged(XamlObject namedObject, string oldName, string newName)
    {
        var obj = namedObject;
        while (obj != null)
        {
            var nameScope = GetNameScopeFromObject(obj);
            if (nameScope != null)
            {
                if (oldName != null)
                    try
                    {
                        // Avalonia INameScope doesn't have Unregister method
                        // We'll try to register with null to effectively unregister
                        if (nameScope.Find(oldName) != null)
                        {
                            // In Avalonia, we can't directly unregister, so we skip this
                            // The new registration will overwrite the old one
                        }
                    }
                    catch (Exception x)
                    {
                        Debug.WriteLine(x.Message);
                    }

                if (newName != null)
                {
                    nameScope.Register(newName, namedObject.Instance);

                    try
                    {
                        var prp = namedObject.ElementType.GetProperty(namedObject.RuntimeNameProperty);
                        if (prp != null)
                            prp.SetValue(namedObject.Instance, newName, null);
                    }
                    catch (Exception x)
                    {
                        Debug.WriteLine(x.Message);
                    }
                }

                break;
            }

            obj = obj.ParentObject;
        }
    }

    /// <summary>
    ///     Gets the XAML namescope for the specified object.
    /// </summary>
    /// <param name="obj">The object to get the XAML namescope for.</param>
    /// <returns>A XAML namescope, as an <see cref="INameScope" /> instance.</returns>
    public static INameScope GetNameScopeFromObject(XamlObject obj)
    {
        INameScope nameScope = null;

        while (obj != null)
        {
            nameScope = obj.Instance as INameScope;
            if (nameScope == null)
            {
                var xamlObj = obj.ParentObject != null ? obj.ParentObject : obj;
                var avaloniaObj = xamlObj.Instance as AvaloniaObject;
                if (avaloniaObj != null)
                {
                    // Try to get namescope from StyledElement first
                    if (avaloniaObj is StyledElement styledElement)
                        nameScope = NameScope.GetNameScope(styledElement);
                    else
                        // For other AvaloniaObjects, try to find a parent StyledElement
                        nameScope = FindNameScopeInParents(xamlObj);
                }

                if (nameScope != null)
                    break;
            }

            obj = obj.ParentObject;
        }

        return nameScope;
    }

    /// <summary>
    ///     Clears the <see cref="NameScope.NameScopeProperty" /> if the object is an <see cref="AvaloniaObject" />.
    /// </summary>
    /// <param name="obj">The object to clear the <see cref="NameScope.NameScopeProperty" /> on.</param>
    public static void ClearNameScopeProperty(object obj)
    {
        var avaloniaObj = obj as AvaloniaObject;
        if (avaloniaObj != null)
            avaloniaObj.ClearValue(NameScope.NameScopeProperty);
    }

    /// <summary>
    ///     Finds a namescope in the parent hierarchy
    /// </summary>
    private static INameScope FindNameScopeInParents(XamlObject xamlObj)
    {
        var current = xamlObj;
        while (current != null)
        {
            if (current.Instance is StyledElement styledElement)
            {
                var nameScope = NameScope.GetNameScope(styledElement);
                if (nameScope != null)
                    return nameScope;
            }
            current = current.ParentObject;
        }
        return null;
    }

    /// <summary>
    ///     Gets a named object from the namescope
    /// </summary>
    public static object GetNamedObject(XamlObject context, string name)
    {
        var nameScope = GetNameScopeFromObject(context);
        return nameScope?.Find(name);
    }
}