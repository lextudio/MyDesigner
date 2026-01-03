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

using System.Globalization;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     Allows animated collapsing of the content of this panel.
/// </summary>
public class CollapsiblePanel : ContentControl
{
    protected override Type StyleKeyOverride => typeof(CollapsiblePanel);

    public static readonly StyledProperty<bool> IsCollapsedProperty = 
        AvaloniaProperty.Register<CollapsiblePanel, bool>(nameof(IsCollapsed), false);

    public static readonly StyledProperty<Orientation> CollapseOrientationProperty =
        AvaloniaProperty.Register<CollapsiblePanel, Orientation>(nameof(CollapseOrientation), Orientation.Vertical);

    public static readonly StyledProperty<TimeSpan> DurationProperty = 
        AvaloniaProperty.Register<CollapsiblePanel, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(250));

    protected internal static readonly StyledProperty<double> AnimationProgressProperty = 
        AvaloniaProperty.Register<CollapsiblePanel, double>(nameof(AnimationProgress), 1.0);

    protected internal static readonly StyledProperty<double> AnimationProgressXProperty = 
        AvaloniaProperty.Register<CollapsiblePanel, double>(nameof(AnimationProgressX), 1.0);

    protected internal static readonly StyledProperty<double> AnimationProgressYProperty = 
        AvaloniaProperty.Register<CollapsiblePanel, double>(nameof(AnimationProgressY), 1.0);

    static CollapsiblePanel()
    {
        IsCollapsedProperty.Changed.AddClassHandler<CollapsiblePanel>((x, e) => x.OnIsCollapsedChanged(e));
        FocusableProperty.OverrideDefaultValue<CollapsiblePanel>(false);
    }

    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public Orientation CollapseOrientation
    {
        get => GetValue(CollapseOrientationProperty);
        set => SetValue(CollapseOrientationProperty, value);
    }

    /// <summary>
    ///     The duration in milliseconds of the animation.
    /// </summary>
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    ///     Value between 0 and 1 specifying how far the animation currently is.
    /// </summary>
    protected internal double AnimationProgress
    {
        get => GetValue(AnimationProgressProperty);
        set => SetValue(AnimationProgressProperty, value);
    }

    /// <summary>
    ///     Value between 0 and 1 specifying how far the animation currently is.
    /// </summary>
    protected internal double AnimationProgressX
    {
        get => GetValue(AnimationProgressXProperty);
        set => SetValue(AnimationProgressXProperty, value);
    }

    /// <summary>
    ///     Value between 0 and 1 specifying how far the animation currently is.
    /// </summary>
    protected internal double AnimationProgressY
    {
        get => GetValue(AnimationProgressYProperty);
        set => SetValue(AnimationProgressYProperty, value);
    }

    private void OnIsCollapsedChanged(AvaloniaPropertyChangedEventArgs e)
    {
        SetupAnimation((bool)e.NewValue);
    }

    private void SetupAnimation(bool isCollapsed)
    {
        if (IsLoaded)
        {
            // If the animation is already running, calculate remaining portion of the time
            var currentProgress = AnimationProgress;
            if (!isCollapsed) currentProgress = 1.0 - currentProgress;

            var animation = new DoubleTransition
            {
                Property = AnimationProgressProperty,
                Duration = TimeSpan.FromSeconds(Duration.TotalSeconds * currentProgress)
            };

            var transitions = new Transitions { animation };

            if (CollapseOrientation == Orientation.Horizontal)
            {
                var animationX = new DoubleTransition
                {
                    Property = AnimationProgressXProperty,
                    Duration = TimeSpan.FromSeconds(Duration.TotalSeconds * currentProgress)
                };
                transitions.Add(animationX);
                AnimationProgressY = 1.0;
            }
            else
            {
                AnimationProgressX = 1.0;
                var animationY = new DoubleTransition
                {
                    Property = AnimationProgressYProperty,
                    Duration = TimeSpan.FromSeconds(Duration.TotalSeconds * currentProgress)
                };
                transitions.Add(animationY);
            }

            Transitions = transitions;
            AnimationProgress = isCollapsed ? 0.0 : 1.0;
        }
        else
        {
            AnimationProgress = isCollapsed ? 0.0 : 1.0;
            AnimationProgressX = CollapseOrientation == Orientation.Horizontal ? AnimationProgress : 1.0;
            AnimationProgressY = CollapseOrientation == Orientation.Vertical ? AnimationProgress : 1.0;
        }
    }
}

internal sealed class CollapsiblePanelProgressToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
            return doubleValue > 0 ? true : false;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SelfCollapsingPanel : CollapsiblePanel
{
    public static readonly StyledProperty<bool> CanCollapseProperty =
        AvaloniaProperty.Register<SelfCollapsingPanel, bool>(nameof(CanCollapse), false);
    protected override Type StyleKeyOverride => typeof(SelfCollapsingPanel);
    static SelfCollapsingPanel()
    {
        CanCollapseProperty.Changed.AddClassHandler<SelfCollapsingPanel>((x, e) => x.OnCanCollapseChanged(e));
    }

    public bool CanCollapse
    {
        get => GetValue(CanCollapseProperty);
        set => SetValue(CanCollapseProperty, value);
    }

    private bool HeldOpenByMouse => IsPointerOver;

    private void OnCanCollapseChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            if (!HeldOpenByMouse)
                IsCollapsed = true;
        }
        else
        {
            IsCollapsed = false;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (CanCollapse && !HeldOpenByMouse)
            IsCollapsed = true;
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        if (CanCollapse && !HeldOpenByMouse)
            IsCollapsed = true;
    }
}