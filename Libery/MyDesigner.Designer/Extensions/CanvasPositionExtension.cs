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
using Avalonia.Media;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

[ExtensionFor(typeof(Control))]
[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
public class CanvasPositionExtension : AdornerProvider
{
    private Canvas _canvas;
    private MarginHandle[] _handles;
    private MarginHandle _leftHandle, _topHandle, _rightHandle, _bottomHandle;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ExtendedItem.Parent != null)
            if (ExtendedItem.Parent.ComponentType == typeof(Canvas))
            {
                var extendedControl = (Control)ExtendedItem.Component;
                var adornerPanel = new AdornerPanel();

                // If the Element is rotated/skewed in the grid, then margin handles do not appear
                var layoutTransform = extendedControl.RenderTransform != null ? extendedControl.RenderTransform.Value : Matrix.Identity;
                var renderTransform = extendedControl.RenderTransform != null ? extendedControl.RenderTransform.Value : Matrix.Identity;
                
                if (layoutTransform == Matrix.Identity && renderTransform == Matrix.Identity)
                {
                    _canvas = ExtendedItem.Parent.View as Canvas;
                    _handles = new[]
                    {
                        _leftHandle = new CanvasPositionHandle(ExtendedItem, adornerPanel, HandleOrientation.Left),
                        _topHandle = new CanvasPositionHandle(ExtendedItem, adornerPanel, HandleOrientation.Top),
                        _rightHandle = new CanvasPositionHandle(ExtendedItem, adornerPanel, HandleOrientation.Right),
                        _bottomHandle = new CanvasPositionHandle(ExtendedItem, adornerPanel, HandleOrientation.Bottom)
                    };
                }

                if (adornerPanel != null)
                    Adorners.Add(adornerPanel);
            }
    }

    public void HideHandles()
    {
        if (_handles != null)
            foreach (var handle in _handles)
            {
                handle.ShouldBeVisible = false;
                handle.IsVisible = false;
            }
    }

    public void ShowHandles()
    {
        if (_handles != null)
            foreach (var handle in _handles)
            {
                handle.ShouldBeVisible = true;
                handle.IsVisible = true;
                handle.DecideVisiblity(handle.HandleLength);
            }
    }
}