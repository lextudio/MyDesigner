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

using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System.Xml;

namespace MyDesigner.XamlDom;

/// <summary>
///     Contains template related helper methods.
/// </summary>
public static class TemplateHelper
{
    /// <summary>
    ///     Gets a <see cref="ITemplate{Control}" /> based on the specified parameters.
    /// </summary>
    /// <param name="xmlElement">The xml element to get template xaml from.</param>
    /// <param name="parentObject">The <see cref="XamlObject" /> to use as source for resources and contextual information.</param>
    /// <returns>A <see cref="ITemplate{Control}" /> based on the specified parameters.</returns>
    public static ITemplate<Control> GetFrameworkTemplate(XmlElement xmlElement, XamlObject parentObject)
    {
        // Simplified implementation for Avalonia
        // In a real implementation, you would parse the XAML and create a proper template
        return new FuncTemplate<Control>(() => new Control());
    }
}