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

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Services;

internal sealed class DefaultErrorService : IErrorService
{
    private readonly ServiceContainer services;

    public DefaultErrorService(DesignContext context)
    {
        services = context.Services;
    }

    public void ShowErrorTooltip(TemplatedControl attachTo, Control errorElement)
    {
        if (attachTo == null)
            throw new ArgumentNullException("attachTo");
        if (errorElement == null)
            throw new ArgumentNullException("errorElement");

        var b = new AttachedErrorBalloon(attachTo, errorElement);
        
        // In Avalonia, positioning is handled differently
        var topLevel = TopLevel.GetTopLevel(attachTo);
        if (topLevel != null)
        {
            var pos = attachTo.TranslatePoint(new Point(0, attachTo.Bounds.Height), topLevel);
            if (pos.HasValue)
            {
                b.Position = new PixelPoint((int)pos.Value.X, (int)(pos.Value.Y - 8));
            }
        }
        
        b.Focusable = false;
        var windowService = services.GetService<ITopLevelWindowService>();
        var ownerWindow = windowService?.GetTopLevelWindow(attachTo);
        
        b.Show();
        if (ownerWindow != null)
        {
            ownerWindow.SetOwner(b);
            ownerWindow.Activate();
        }
        b.AttachEvents();
    }

    private sealed class AttachedErrorBalloon : ErrorBalloon
    {
        private readonly Control attachTo;

        public AttachedErrorBalloon(Control attachTo, Control errorElement)
        {
            this.attachTo = attachTo;
            Content = errorElement;
        }

        internal void AttachEvents()
        {
            attachTo.Unloaded += OnCloseEvent;
            attachTo.AddHandler(InputElement.KeyDownEvent, OnCloseEvent, handledEventsToo: true);
            attachTo.AddHandler(InputElement.PointerPressedEvent, OnCloseEvent, handledEventsToo: true);
            attachTo.LostFocus += OnCloseEvent;
        }

        private void OnCloseEvent(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            attachTo.Unloaded -= OnCloseEvent;
            attachTo.RemoveHandler(InputElement.KeyDownEvent, OnCloseEvent);
            attachTo.RemoveHandler(InputElement.PointerPressedEvent, OnCloseEvent);
            attachTo.LostFocus -= OnCloseEvent;
            base.OnClosing(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Close();
        }
    }
}