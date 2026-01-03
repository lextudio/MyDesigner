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

namespace MyDesigner.Designer.Extensions;

/// <summary>
/// Extension methods for Thickness to provide With* methods for Avalonia 11.x compatibility
/// </summary>
public static class ThicknessExtensions
{
    public static Thickness WithLeft(this Thickness thickness, double left)
    {
        return new Thickness(left, thickness.Top, thickness.Right, thickness.Bottom);
    }

    public static Thickness WithTop(this Thickness thickness, double top)
    {
        return new Thickness(thickness.Left, top, thickness.Right, thickness.Bottom);
    }

    public static Thickness WithRight(this Thickness thickness, double right)
    {
        return new Thickness(thickness.Left, thickness.Top, right, thickness.Bottom);
    }

    public static Thickness WithBottom(this Thickness thickness, double bottom)
    {
        return new Thickness(thickness.Left, thickness.Top, thickness.Right, bottom);
    }
}