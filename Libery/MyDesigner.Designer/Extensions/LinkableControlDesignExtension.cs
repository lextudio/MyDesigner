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
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using HMIControl.Base.HMIBase;
using HMIControl.HMI.Model;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
[ExtensionFor(typeof(LinkableControl))]
public class LinkableControlDesignExtension : SelectionAdornerProvider
{
    private static readonly Dictionary<ConnectOrientation, string> omap = new()
    {
        { ConnectOrientation.Left, "LeftPin" },
        { ConnectOrientation.Right, "RightPin" },
        { ConnectOrientation.Top, "TopPin" },
        { ConnectOrientation.Bottom, "BottomPin" },
        { ConnectOrientation.None, string.Empty }
    };

    private readonly AdornerPanel _panel = new();
    private ControlAdorner? adorner;
    private Panel? designCanvas;

    private LinkableControl? designObject;
    private LinkPin? hitPin;
    private bool isdrag;

    protected override void OnInitialized()
    {
        if (ExtendedItem.Component is WindowClone)
            return;
        base.OnInitialized();

        designObject = ExtendedItem.Component as LinkableControl;

        if (designObject != null)
        {
            designCanvas = designObject.Parent as Panel;

            adorner = new ControlAdorner(designObject);
            adorner.RenderTransform = new ScaleTransform(1.0, 1.0);
            foreach (var pin in adorner.Children)
            {
                pin.PointerPressed += Pin_PointerPressed; //按下左键选中hitPin，开始拖动
                pin.PointerMoved += Pin_PointerMoved; //移动鼠标，开始找寻目标连接节点
            }

            adorner.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            adorner.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

            _panel.Children.Add(adorner);

            Adorners.Add(_panel);
        }
    }

    private void Pin_PointerPressed(object? s, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
        {
            hitPin = s as LinkPin;
            isdrag = true;
            e.Handled = true;
        }
    }

    private void Pin_PointerMoved(object? s, PointerEventArgs e)
    {
        var currentPoint = e.GetCurrentPoint(null);
        if (!currentPoint.Properties.IsLeftButtonPressed)
            isdrag = false;

        if (isdrag && hitPin != null && designCanvas != null)
        {
            var pinAdorner = new PinAdorner(designCanvas, hitPin);
            pinAdorner.PointerReleased += pinAdorner_PointerReleased;

            var zoom = 1.0d;
            pinAdorner.RenderTransform = new ScaleTransform(zoom, zoom);
            _panel.Children.Add(pinAdorner);
            e.Handled = true;
        }
    }

    private void pinAdorner_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var pinAdorner = sender as PinAdorner;
        if (pinAdorner?.HitPin != null)
        {
            var targetObject = pinAdorner.HitLinkableControl;
            var lp1 = pinAdorner.SourcePin;
            var lp2 = pinAdorner.HitPin;

            var info1 = lp1.GetInfo();
            var info2 = lp2.GetInfo();

            var line = new LinkLine(info1, info2);

            var customControl = ExtendedItem.Services.Component.RegisterComponentForDesigner(line);

            customControl.Properties.GetProperty(LinkLine.OriginInfoProperty).SetValue(new Binding());
            customControl.Properties.GetProperty(LinkLine.OriginInfoProperty).Value.Properties["Path"].SetValue(
                $"{omap[info1.Orient]}");
            customControl.Properties.GetProperty(LinkLine.OriginInfoProperty).Value.Properties["ElementName"].SetValue(
                $"{designObject?.Name}");

            customControl.Properties.GetProperty(LinkLine.TargetInfoProperty).SetValue(new Binding());
            customControl.Properties.GetProperty(LinkLine.TargetInfoProperty).Value.Properties["Path"].SetValue(
                $"{omap[info2.Orient]}");
            customControl.Properties.GetProperty(LinkLine.TargetInfoProperty).Value.Properties["ElementName"].SetValue(
                $"{targetObject?.Name}");

            ExtendedItem.Parent.ContentProperty.CollectionElements.Add(customControl);
        }

        if (pinAdorner?.HitLinkableControl != null) 
            pinAdorner.HitLinkableControl.IsLinkDragOver = false;

        hitPin = null;
        // Note: Avalonia doesn't have IsMouseCaptured/ReleaseMouseCapture equivalent
        // This would need to be handled differently
        if (pinAdorner != null)
            _panel.Children.Remove(pinAdorner);
    }
}