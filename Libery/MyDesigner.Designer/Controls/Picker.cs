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
using Avalonia.Layout;
using Avalonia.Media;

namespace MyDesigner.Designer.Controls;

public class Picker : Grid
{
    protected override Type StyleKeyOverride => typeof(Picker);
    public static readonly StyledProperty<Control> MarkerProperty =
        AvaloniaProperty.Register<Picker, Control>(nameof(Marker));

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<Picker, double>(nameof(Value), 0.0, 
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> ValueOffsetProperty =
        AvaloniaProperty.Register<Picker, double>(nameof(ValueOffset));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Picker, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<Picker, double>(nameof(Minimum));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<Picker, double>(nameof(Maximum), 100.0);

    private bool isPointerDown;

    public Picker()
    {
        SizeChanged += delegate { UpdateValueOffset(); };
    }

    public Control Marker
    {
        get => GetValue(MarkerProperty);
        set => SetValue(MarkerProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double ValueOffset
    {
        get => GetValue(ValueOffsetProperty);
        set => SetValue(ValueOffsetProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MarkerProperty)
        {
            var marker = change.GetNewValue<Control>();
            if (marker != null)
            {
                var t = marker.RenderTransform as TranslateTransform;
                if (t == null)
                {
                    t = new TranslateTransform();
                    marker.RenderTransform = t;
                }

                var property = Orientation == Orientation.Horizontal
                    ? TranslateTransform.XProperty
                    : TranslateTransform.YProperty;
                    
                t.Bind(property, new Binding
                {
                    Source = this,
                    Path = nameof(ValueOffset)
                });
            }
        }
        else if (change.Property == ValueProperty)
        {
            UpdateValueOffset();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        isPointerDown = true;
        e.Pointer.Capture(this);
        UpdateValue(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (isPointerDown) UpdateValue(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        isPointerDown = false;
        e.Pointer.Capture(null);
    }

    private void UpdateValue(PointerEventArgs e)
    {
        var p = e.GetPosition(this);
        double length = 0, pos = 0;

        if (Orientation == Orientation.Horizontal)
        {
            length = Bounds.Width;
            pos = p.X;
        }
        else
        {
            length = Bounds.Height;
            pos = p.Y;
        }

        pos = Math.Max(0, Math.Min(length, pos));
        Value = Minimum + (Maximum - Minimum) * pos / length;
    }

    private void UpdateValueOffset()
    {
        var length = Orientation == Orientation.Horizontal ? Bounds.Width : Bounds.Height;
        if (length > 0)
        {
            ValueOffset = length * (Value - Minimum) / (Maximum - Minimum);
        }
    }
}