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
using System.Reflection;
using Avalonia;
using Avalonia.Media;
using MyDesigner.Design.PropertyGrid;

namespace MyDesigner.Designer.PropertyGrid.Editors.BrushEditor;

public class BrushEditor : INotifyPropertyChanged
{
    // Note: Avalonia doesn't have SystemColors/SystemBrushes like WPF
    // We'll create basic color collections instead
    public static BrushItem[] AvaloniaColors = typeof(Colors)
        .GetProperties(BindingFlags.Static | BindingFlags.Public)
        .Where(p => p.PropertyType == typeof(Color))
        .Select(p => new BrushItem { Name = p.Name, Brush = new SolidColorBrush((Color)p.GetValue(null, null)) })
        .ToArray();

    public static BrushItem[] AvaloniaBrushes = typeof(Brushes)
        .GetProperties(BindingFlags.Static | BindingFlags.Public)
        .Where(p => p.PropertyType == typeof(ISolidColorBrush))
        .Select(p => new BrushItem { Name = p.Name, Brush = (IBrush)p.GetValue(null, null) })
        .ToArray();

    private BrushEditorKind currentKind;
    private LinearGradientBrush linearGradientBrush;

    private PropertyNode property;
    private RadialGradientBrush radialGradientBrush;

    private SolidColorBrush solidColorBrush = new(Colors.White);

    public BrushEditor()
    {
        var stops = new GradientStops();
        stops.Add(new GradientStop(Colors.Black, 0));
        stops.Add(new GradientStop(Colors.White, 1));

        linearGradientBrush = new LinearGradientBrush();
        linearGradientBrush.GradientStops = stops;
        linearGradientBrush.EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative);
        
        radialGradientBrush = new RadialGradientBrush();
        radialGradientBrush.GradientStops = stops;
    }

    public PropertyNode Property
    {
        get => property;
        set
        {
            property = value;
            // Note: Avalonia doesn't have Freezable concept like WPF
            // We'll handle immutability differently if needed

            DetermineCurrentKind();
            RaisePropertyChanged("Property");
            RaisePropertyChanged("Brush");
        }
    }

    public IBrush Brush
    {
        get
        {
            if (property != null) return property.Value as IBrush;
            return null;
        }
        set
        {
            if (property != null && property.Value != value)
            {
                property.Value = value;
                DetermineCurrentKind();
                RaisePropertyChanged("Brush");
            }
        }
    }

    public Color Color
    {
        get => Brush is ISolidColorBrush ? ((ISolidColorBrush)Brush).Color : Colors.Black;

        set
        {
            if (Brush is SolidColorBrush solidBrush)
                solidBrush.Color = value;
            else
                Brush = new SolidColorBrush(value);
        }
    }

    public BrushEditorKind CurrentKind
    {
        get => currentKind;
        set
        {
            currentKind = value;
            RaisePropertyChanged("CurrentKind");

            switch (CurrentKind)
            {
                case BrushEditorKind.None:
                    Brush = null;
                    break;

                case BrushEditorKind.Solid:
                    Brush = solidColorBrush;
                    break;

                case BrushEditorKind.Linear:
                    Brush = linearGradientBrush;
                    break;

                case BrushEditorKind.Radial:
                    Brush = radialGradientBrush;
                    break;

                case BrushEditorKind.List:
                    Brush = solidColorBrush;
                    break;
            }
        }
    }

    public double GradientAngle
    {
        get
        {
            var endPoint = linearGradientBrush.EndPoint;
            var startPoint = linearGradientBrush.StartPoint;
            
            var x = endPoint.Point.X - startPoint.Point.X;
            var y = endPoint.Point.Y - startPoint.Point.Y;
            
            return Math.Atan2(-y, x) * 180 / Math.PI;
        }
        set
        {
            var d = value * Math.PI / 180;
            var p = new Point(Math.Cos(d), -Math.Sin(d));
            var k = 1 / Math.Max(Math.Abs(p.X), Math.Abs(p.Y));
            p = new Point(p.X * k, p.Y * k);
            var p2 = new Point(-p.X, -p.Y);
            
            linearGradientBrush.StartPoint = new RelativePoint((p2.X + 1) / 2, (p2.Y + 1) / 2, RelativeUnit.Relative);
            linearGradientBrush.EndPoint = new RelativePoint((p.X + 1) / 2, (p.Y + 1) / 2, RelativeUnit.Relative);
            RaisePropertyChanged("GradientAngle");
        }
    }

    public IEnumerable<BrushItem> AvailableAvaloniaBrushes => AvaloniaBrushes;

    private void DetermineCurrentKind()
    {
        if (Brush == null)
        {
            CurrentKind = BrushEditorKind.None;
        }
        else if (Brush is ISolidColorBrush)
        {
            solidColorBrush = Brush as SolidColorBrush ?? new SolidColorBrush(((ISolidColorBrush)Brush).Color);
            CurrentKind = BrushEditorKind.Solid;
        }
        else if (Brush is LinearGradientBrush)
        {
            linearGradientBrush = Brush as LinearGradientBrush;
            radialGradientBrush.GradientStops = linearGradientBrush.GradientStops;
            CurrentKind = BrushEditorKind.Linear;
        }
        else if (Brush is RadialGradientBrush)
        {
            radialGradientBrush = Brush as RadialGradientBrush;
            linearGradientBrush.GradientStops = radialGradientBrush.GradientStops;
            CurrentKind = BrushEditorKind.Radial;
        }
    }

    public void MakeGradientHorizontal()
    {
        GradientAngle = 0;
    }

    public void MakeGradientVertical()
    {
        GradientAngle = -90;
    }

    public void Commit()
    {
        // تطبيق التغييرات على الخاصية
        if (Property != null && Brush != null) 
        {
            try
            {
                Property.Value = Brush;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error committing brush changes: {ex.Message}");
            }
        }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    #endregion
}

public enum BrushEditorKind
{
    None,
    Solid,
    Linear,
    Radial,
    List
}

public class BrushItem
{
    public string Name { get; set; }
    public IBrush Brush { get; set; }
}