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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyDesigner.Design.Extensions;

/// <summary>
/// XAML Instance Factory for Avalonia
/// </summary>
[ExtensionServer(typeof(NeverApplyExtensionsExtensionServer))]
public class XamlInstanceFactory : Extension
{
    /// <summary>
    ///     Gets a default instance factory that uses Activator.CreateInstance to create instances.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly XamlInstanceFactory DefaultInstanceFactory = new();

    /// <summary>
    ///     Creates a new XamlInstanceFactory instance.
    /// </summary>
    protected XamlInstanceFactory()
    {
    }

    /// <summary>
    ///     A Instance Factory that uses XAML to instantiate the Control for Avalonia!
    ///     Uses AvaloniaRuntimeXamlLoader equivalent to WPF's XamlServices.Load(new XamlXmlReader(new StringReader(xaml)))
    /// </summary>
    public virtual object CreateInstance(Type type, params object[] arguments)
    {
        var txt = @"<ContentControl xmlns=""https://github.com/avaloniaui"">
<ContentControl.Resources>
<ResourceDictionary>
</ResourceDictionary>
</ContentControl.Resources>
<a:{0} xmlns:a=""clr-namespace:{1};assembly={2}"" />
</ContentControl>";

        var xaml = string.Format(txt, type.Name, type.Namespace, type.Assembly.GetName().Name);
        
        try
        {
            // Load the result using Avalonia runtime XAML loader
            var xmlReader = XmlReader.Create(new StringReader(xaml));
            var ctl = new RuntimeXamlLoaderDocument(xaml);
            var contentControl = AvaloniaRuntimeXamlLoader.Load(ctl) as ContentControl;
            return contentControl?.Content;
        }
        catch
        {
            // Fallback to regular instantiation if XAML parsing fails
            return Activator.CreateInstance(type, arguments);
        }
    }
}