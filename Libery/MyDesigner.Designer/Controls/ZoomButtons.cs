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
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace MyDesigner.Designer.Controls;

public class ZoomButtons : RangeBase
{
    private const double ZoomFactor = 1.1;
    protected override Type StyleKeyOverride => typeof(ZoomButtons);
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var uxPlus = e.NameScope.Find<Button>("uxPlus");
        var uxMinus = e.NameScope.Find<Button>("uxMinus");
        var uxReset = e.NameScope.Find<Button>("uxReset");
        var ux100Percent = e.NameScope.Find<Button>("ux100Percent");

        if (uxPlus != null)
            uxPlus.Click += OnZoomInClick;
        if (uxMinus != null)
            uxMinus.Click += OnZoomOutClick;
        if (uxReset != null)
            uxReset.Click += OnResetClick;
        if (ux100Percent != null)
            ux100Percent.Click += On100PercentClick;
    }

    private void OnZoomInClick(object sender, RoutedEventArgs e)
    {
        Value = ZoomScrollViewer.RoundToOneIfClose(Value * ZoomFactor);
    }

    private void OnZoomOutClick(object sender, RoutedEventArgs e)
    {
        Value = ZoomScrollViewer.RoundToOneIfClose(Value / ZoomFactor);
    }

    private void OnResetClick(object sender, RoutedEventArgs e)
    {
        Value = 1.0;
    }

    private void On100PercentClick(object sender, RoutedEventArgs e)
    {
        var zctl = this.FindAncestorOfType<ZoomControl>();
        if (zctl?.Content is Control content)
        {
            var contentWidth = content.Bounds.Width;
            var contentHeight = content.Bounds.Height;
            var width = zctl.Bounds.Width;
            var height = zctl.Bounds.Height;

            if (contentWidth > width || contentHeight > height)
            {
                var widthProportion = contentWidth / width;
                var heightProportion = contentHeight / height;

                if (widthProportion > heightProportion)
                    Value = (width - 20.00) / contentWidth;
                else
                    Value = (height - 20.00) / contentHeight;
            }
        }
    }
}