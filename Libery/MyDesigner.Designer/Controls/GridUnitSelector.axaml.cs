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
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     Interaction logic for GridUnitSelector.xaml
/// </summary>
public partial class GridUnitSelector : UserControl
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<GridUnitSelector, Orientation>(nameof(Orientation));

    private readonly GridRailAdorner rail;
   
   

    public GridUnitSelector(GridRailAdorner rail)
    {
        InitializeComponent();
        this.rail = rail;
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public DesignItem SelectedItem { get; set; }

    public GridUnitType Unit
    {
        get
        {
            if (autoRadio.IsChecked == true)
                return GridUnitType.Auto;
            if (starRadio.IsChecked == true)
                return GridUnitType.Star;

            return GridUnitType.Pixel;
        }
        set
        {
            switch (value)
            {
                case GridUnitType.Auto:
                    autoRadio.IsChecked = true;
                    break;
                case GridUnitType.Star:
                    starRadio.IsChecked = true;
                    break;
                default:
                    fixedRadio.IsChecked = true;
                    break;
            }
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        autoRadio = this.FindControl<RadioButton>("autoRadio");
        starRadio = this.FindControl<RadioButton>("starRadio");
        fixedRadio = this.FindControl<RadioButton>("fixedRadio");

        fixedRadio.IsCheckedChanged += FixedChecked;
        starRadio.IsCheckedChanged += StarChecked;
        autoRadio.IsCheckedChanged += AutoChecked;
    }

    private void FixedChecked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        rail.SetGridLengthUnit(Unit);
    }

    private void StarChecked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        rail.SetGridLengthUnit(Unit);
    }

    private void AutoChecked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        rail.SetGridLengthUnit(Unit);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        IsVisible = false;
    }
}