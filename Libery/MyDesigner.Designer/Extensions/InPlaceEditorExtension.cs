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
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Design.UIExtensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     Extends In-Place editor to edit any text in the designer which is wrapped in the Visual tree under TexBlock
/// </summary>
[ExtensionFor(typeof(TextBlock))]
public class InPlaceEditorExtension : PrimarySelectionAdornerProvider
{
    private readonly AdornerPanel adornerPanel;
    private DesignPanel designPanel;
    private InPlaceEditor editor;
    private Control element;
    private bool eventsAdded;
    private bool isGettingDragged;
    private bool isPointerDown;
    private int numClicks;
    private RelativePlacement placement;
    private TextBlock textBlock;

    public InPlaceEditorExtension()
    {
        adornerPanel = new AdornerPanel();
        isGettingDragged = false;
        isPointerDown = false;
        numClicks = 0;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        element = ExtendedItem.Component as Control;
        editor = new InPlaceEditor(ExtendedItem);
        editor.DataContext = element;
        editor.IsVisible = false;

        placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
        adornerPanel.Children.Add(editor);
        Adorners.Add(adornerPanel);

        designPanel = ExtendedItem.Services.GetService<IDesignPanel>() as DesignPanel;
        Debug.Assert(designPanel != null);

        designPanel.AddHandler(InputElement.PointerPressedEvent, PointerPressed, handledEventsToo: true);
        designPanel.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, handledEventsToo: true);
        designPanel.AddHandler(InputElement.PointerMovedEvent, PointerMoved, handledEventsToo: true);

        ExtendedItem.PropertyChanged += PropertyChanged;
        eventsAdded = true;
    }

    private void PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (textBlock != null)
        {
            if (e.PropertyName == "Width")
            {
                // Update placement based on new width
                editor.MaxWidth = Math.Max(ModelTools.GetWidth(element) - placement.XOffset, 0);
            }

            if (e.PropertyName == "Height")
            {
                // Update placement based on new height  
                editor.MaxHeight = Math.Max(ModelTools.GetHeight(element) - placement.YOffset, 0);
            }

            AdornerPanel.SetPlacement(editor, placement);
        }
    }

    private void PlaceEditor(Visual text, PointerEventArgs e)
    {
        textBlock = text as TextBlock;
        Debug.Assert(textBlock != null);

        var elementPos = e.GetPosition(element);
        var textPos = e.GetPosition(textBlock);
        
        placement.XOffset = elementPos.X - textPos.X - 2.8;
        placement.YOffset = elementPos.Y - textPos.Y - 1;
        placement.XRelativeToAdornerWidth = 0;
        placement.XRelativeToContentWidth = 0;
        placement.YRelativeToAdornerHeight = 0;
        placement.YRelativeToContentHeight = 0;

        editor.DataContext = textBlock;

        editor.Bind(Control.WidthProperty, new Binding("Bounds.Width") { Source = textBlock });
        editor.Bind(Control.HeightProperty, new Binding("Bounds.Height") { Source = textBlock });

        textBlock.IsVisible = false;
        AdornerPanel.SetPlacement(editor, placement);
        RemoveBorder();
    }

    public void AbortEdit()
    {
        editor.AbortEditing();
    }

    public void StartEdit()
    {
        editor.StartEditing();
    }

    protected override void OnRemove()
    {
        RemoveEventsAndShowControl();
        base.OnRemove();
    }

    private void RemoveEventsAndShowControl()
    {
        editor.IsVisible = false;
        if (textBlock != null) textBlock.IsVisible = true;

        if (eventsAdded)
        {
            eventsAdded = false;
            ExtendedItem.PropertyChanged -= PropertyChanged;
            designPanel.RemoveHandler(InputElement.PointerPressedEvent, PointerPressed);
            designPanel.RemoveHandler(InputElement.PointerMovedEvent, PointerMoved);
            designPanel.RemoveHandler(InputElement.PointerReleasedEvent, PointerReleased);
        }
    }

    #region PointerEvents

    private DesignPanelHitTestResult result;
    private Point Current;
    private Point Start;

    private void PointerPressed(object sender, PointerPressedEventArgs e)
    {
        result = designPanel.HitTest(e.GetPosition(designPanel as Visual), false, true, HitTestType.Default);
        if (result.ModelHit == ExtendedItem && result.VisualHit is TextBlock)
        {
            Start = e.GetCurrentPoint(null).Position;
            Current = Start;
            isPointerDown = true;
        }
        numClicks++;
    }

    private void PointerMoved(object sender, PointerEventArgs e)
    {
        var currentPos = e.GetCurrentPoint(null).Position;
        Current = currentPos;
        result = designPanel.HitTest(e.GetPosition(designPanel as Visual), false, true, HitTestType.Default);
        
        if (result.ModelHit == ExtendedItem && result.VisualHit is TextBlock)
        {
            if (numClicks > 0)
            {
                const double MinDragDistance = 4.0; // Avalonia doesn't have SystemParameters
                if (isPointerDown &&
                    (Math.Abs(Current.X - Start.X) > MinDragDistance ||
                     Math.Abs(Current.Y - Start.Y) > MinDragDistance))
                {
                    isGettingDragged = true;
                    editor.Focus();
                }
            }

            DrawBorder((Control)result.VisualHit);
        }
        else
        {
            RemoveBorder();
        }
    }

    private void PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        result = designPanel.HitTest(e.GetPosition(designPanel as Visual), true, true, HitTestType.Default);
        if (((result.ModelHit == ExtendedItem && result.VisualHit is TextBlock) || 
             (result.VisualHit != null && result.VisualHit.FindAncestorOfType<InPlaceEditor>() == editor)) && 
            numClicks > 0)
        {
            if (!isGettingDragged)
            {
                PlaceEditor(ExtendedItem.View, e);
                foreach (var extension in ExtendedItem.Extensions)
                    if (!(extension is InPlaceEditorExtension) && !(extension is SelectedElementRectangleExtension))
                        ExtendedItem.RemoveExtension(extension);

                editor.IsVisible = true;
            }
        }
        else
        {
            RemoveEventsAndShowControl();
            ExtendedItem.ReapplyAllExtensions();
        }

        isPointerDown = false;
        isGettingDragged = false;
    }

    #endregion

    #region HighlightBorder

    private Border _border;

    private sealed class BorderPlacement : AdornerPlacement
    {
        private readonly Control _element;

        public BorderPlacement(Control element)
        {
            _element = element;
        }

        public override void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize)
        {
            var transform = MyDesigner.Design.UIExtensions.VisualExtensions.TransformToVisual(_element, panel.AdornedElement);
            var p = transform.Transform(new Point());
            var rect = new Rect(p, _element.Bounds.Size);
            rect = rect.Inflate(new Thickness(3, 1, 3, 1));
            adorner.Arrange(rect);
        }
    }

    private void DrawBorder(Control item)
    {
        if (editor != null && !editor.IsVisible)
        {
            if (adornerPanel.Children.Contains(_border))
                adornerPanel.Children.Remove(_border);
            _border = new Border
            {
                BorderBrush = Brushes.Gray, 
                BorderThickness = new Thickness(1.4)
            };
            ToolTip.SetTip(_border, "Edit this Text");
            
            var bp = new BorderPlacement(item);
            AdornerPanel.SetPlacement(_border, bp);
            adornerPanel.Children.Add(_border);
        }
    }

    private void RemoveBorder()
    {
        if (adornerPanel.Children.Contains(_border))
            adornerPanel.Children.Remove(_border);
    }

    #endregion
}