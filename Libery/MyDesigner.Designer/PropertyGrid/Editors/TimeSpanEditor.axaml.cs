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
using Avalonia.Markup.Xaml;
using MyDesigner.Design.PropertyGrid;

namespace MyDesigner.Designer.PropertyGrid.Editors;

[TypeEditor(typeof(TimeSpan))]
public partial class TimeSpanEditor : UserControl
{
    // Using StyledProperty as the backing store for Negative
    public static readonly StyledProperty<bool> NegativeProperty =
        AvaloniaProperty.Register<TimeSpanEditor, bool>(nameof(Negative));

    // Using StyledProperty as the backing store for Days
    public static readonly StyledProperty<int> DaysProperty =
        AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Days));

    // Using StyledProperty as the backing store for Hours
    public static readonly StyledProperty<int> HoursProperty =
        AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Hours));

    // Using StyledProperty as the backing store for Minutes
    public static readonly StyledProperty<int> MinutesProperty =
        AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Minutes));

    // Using StyledProperty as the backing store for Seconds
    public static readonly StyledProperty<int> SecondsProperty =
        AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Seconds));

    // Using StyledProperty as the backing store for Milliseconds
    public static readonly StyledProperty<int> MillisecondsProperty =
        AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Milliseconds));

    static TimeSpanEditor()
    {
        NegativeProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => x.UpdateValue());
        DaysProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => x.UpdateValue());
        HoursProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => x.OnHoursChanged());
        MinutesProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => x.OnMinutesChanged());
        SecondsProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => x.OnSecondsChanged());
        MillisecondsProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => x.OnMillisecondsChanged());
    }

    public TimeSpanEditor()
    {
        InitializeComponent();
        DataContextChanged += NumberEditor_DataContextChanged;
    }

    public PropertyNode PropertyNode => DataContext as PropertyNode;

    public bool Negative
    {
        get => GetValue(NegativeProperty);
        set => SetValue(NegativeProperty, value);
    }

    public int Days
    {
        get => GetValue(DaysProperty);
        set => SetValue(DaysProperty, value);
    }

    public int Hours
    {
        get => GetValue(HoursProperty);
        set => SetValue(HoursProperty, value);
    }

    public int Minutes
    {
        get => GetValue(MinutesProperty);
        set => SetValue(MinutesProperty, value);
    }

    public int Seconds
    {
        get => GetValue(SecondsProperty);
        set => SetValue(SecondsProperty, value);
    }

    public int Milliseconds
    {
        get => GetValue(MillisecondsProperty);
        set => SetValue(MillisecondsProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void NumberEditor_DataContextChanged(object sender, EventArgs e)
    {
        if (PropertyNode == null)
            return;

        var designerValue = PropertyNode.DesignerValue;
        if (designerValue == null)
            return;

        var value = (TimeSpan)designerValue;

        if (value < TimeSpan.Zero)
        {
            Negative = true;
            value = value.Negate();
        }

        Days = value.Days;
        Hours = value.Hours;
        Minutes = value.Minutes;
        Seconds = value.Seconds;
        Milliseconds = value.Milliseconds;
    }

    private void UpdateValue()
    {
        if (PropertyNode == null) return;
        
        var ts = new TimeSpan(Days, Hours, Minutes, Seconds, Milliseconds);
        if (Negative)
            ts = ts.Negate();
        PropertyNode.DesignerValue = ts;
    }

    private void OnHoursChanged()
    {
        if (Hours > 23)
        {
            Days++;
            Hours = 0;
        }
        else if (Hours < 0)
        {
            Days--;
            Hours = 23;
        }
        UpdateValue();
    }

    private void OnMinutesChanged()
    {
        if (Minutes > 59)
        {
            Hours++;
            Minutes = 0;
        }
        else if (Minutes < 0)
        {
            Hours--;
            Minutes = 59;
        }
        UpdateValue();
    }

    private void OnSecondsChanged()
    {
        if (Seconds > 59)
        {
            Minutes++;
            Seconds = 0;
        }
        else if (Seconds < 0)
        {
            Minutes--;
            Seconds = 59;
        }
        UpdateValue();
    }

    private void OnMillisecondsChanged()
    {
        if (Milliseconds > 999)
        {
            Seconds++;
            Milliseconds = 0;
        }
        else if (Milliseconds < 0)
        {
            Seconds--;
            Milliseconds = 999;
        }
        UpdateValue();
    }
}