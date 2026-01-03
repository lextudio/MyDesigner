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

using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Input;

namespace MyDesigner.Designer;

public static class BasicMetadata
{
    private static bool registered;

    public static void Register()
    {
        if (registered) return;
        registered = true;

        // Avalonia equivalents for WPF metadata registration
        Metadata.AddStandardValues(typeof(IBrush), typeof(Brushes));
        Metadata.AddStandardValues(typeof(Color), typeof(Colors));
        Metadata.AddStandardValues(typeof(FontWeight), typeof(FontWeight));
        Metadata.AddStandardValues(typeof(FontStyle), typeof(FontStyle));
        // Font families will be handled differently in Avalonia
        Metadata.AddStandardValues(typeof(FontFamily), new string[] { "Arial", "Times New Roman", "Courier New" });

        // Popular properties for Avalonia controls
        Metadata.AddPopularProperty(Line.EndPointProperty);
        Metadata.AddPopularProperty(Line.StartPointProperty);
        Metadata.AddPopularProperty(Polygon.PointsProperty);
        Metadata.AddPopularProperty(Polyline.PointsProperty);
        Metadata.AddPopularProperty(Avalonia.Controls.Shapes.Path.DataProperty);
        Metadata.AddPopularProperty(HeaderedContentControl.HeaderProperty);
        Metadata.AddPopularProperty(Shape.FillProperty);
        Metadata.AddPopularProperty(ItemsControl.ItemsSourceProperty);
        Metadata.AddPopularProperty(Image.SourceProperty);
        Metadata.AddPopularProperty(TextBlock.TextProperty);
        Metadata.AddPopularProperty(DockPanel.LastChildFillProperty);
        Metadata.AddPopularProperty(Expander.IsExpandedProperty);
        Metadata.AddPopularProperty(Shape.StrokeProperty);
        Metadata.AddPopularProperty(RangeBase.ValueProperty);
        // ItemContainerStyleProperty doesn't exist in Avalonia
        // Metadata.AddPopularProperty(ItemsControl.ItemContainerStyleProperty);
        Metadata.AddPopularProperty(ToggleButton.IsCheckedProperty);
        Metadata.AddPopularProperty(Window.TitleProperty);
        Metadata.AddPopularProperty(Rectangle.RadiusXProperty);
        Metadata.AddPopularProperty(Rectangle.RadiusYProperty);
        Metadata.AddPopularProperty(Layoutable.HeightProperty);
        Metadata.AddPopularProperty(Layoutable.WidthProperty);
        Metadata.AddPopularProperty(RangeBase.MinimumProperty);
        Metadata.AddPopularProperty(RangeBase.MaximumProperty);
        Metadata.AddPopularProperty(ScrollBar.OrientationProperty);
        Metadata.AddPopularProperty(ContentControl.ContentProperty);
        Metadata.AddPopularProperty(Popup.IsOpenProperty);
        // TextElement doesn't exist in Avalonia, using TextBlock instead
        Metadata.AddPopularProperty(TextBlock.FontSizeProperty);
        Metadata.AddPopularProperty(StyledElement.NameProperty);
        Metadata.AddPopularProperty(Shape.StrokeThicknessProperty);
        Metadata.AddPopularProperty(TextBlock.ForegroundProperty);
        Metadata.AddPopularProperty(Layoutable.VerticalAlignmentProperty);
        Metadata.AddPopularProperty(Button.IsDefaultProperty);
        Metadata.AddPopularProperty(Visual.RenderTransformOriginProperty);
        Metadata.AddPopularProperty(TextBlock.FontFamilyProperty);
        Metadata.AddPopularProperty(Layoutable.HorizontalAlignmentProperty);
        Metadata.AddPopularProperty(ItemsControl.ItemTemplateProperty);
        Metadata.AddPopularProperty(TextBlock.TextWrappingProperty);
        Metadata.AddPopularProperty(Layoutable.MarginProperty);
        Metadata.AddPopularProperty(Panel.BackgroundProperty);
        Metadata.AddPopularProperty(TextBlock.FontWeightProperty);
        Metadata.AddPopularProperty(StackPanel.OrientationProperty);
        Metadata.AddPopularProperty(ListBox.SelectionModeProperty);
        // StylesProperty doesn't exist in Avalonia
        // Metadata.AddPopularProperty(StyledElement.StylesProperty);
        Metadata.AddPopularProperty(TextBox.TextProperty);
        Metadata.AddPopularProperty(Window.SizeToContentProperty);
        Metadata.AddPopularProperty(Window.CanResizeProperty);
        Metadata.AddPopularProperty(TextBlock.TextTrimmingProperty);
        Metadata.AddPopularProperty(Window.ShowInTaskbarProperty);
        Metadata.AddPopularProperty(Window.IconProperty);
        Metadata.AddPopularProperty(Visual.RenderTransformProperty);
        Metadata.AddPopularProperty(Button.IsCancelProperty);
        Metadata.AddPopularProperty(Border.BorderBrushProperty);
        Metadata.AddPopularProperty(Border.CornerRadiusProperty);
        Metadata.AddPopularProperty(Border.BorderThicknessProperty);
        Metadata.AddPopularProperty(Border.PaddingProperty);
        Metadata.AddPopularProperty(Shape.StretchProperty);
        Metadata.AddPopularProperty(ContentControl.VerticalContentAlignmentProperty);
        Metadata.AddPopularProperty(ContentControl.HorizontalContentAlignmentProperty);

        // Attached properties
        Metadata.AddPopularProperty(Grid.RowProperty);
        Metadata.AddPopularProperty(Grid.RowSpanProperty);
        Metadata.AddPopularProperty(Grid.ColumnProperty);
        Metadata.AddPopularProperty(Grid.ColumnSpanProperty);
        Metadata.AddPopularProperty(DockPanel.DockProperty);
        Metadata.AddPopularProperty(Canvas.LeftProperty);
        Metadata.AddPopularProperty(Canvas.TopProperty);
        Metadata.AddPopularProperty(Canvas.RightProperty);
        Metadata.AddPopularProperty(Canvas.BottomProperty);

        // Binding properties
        Metadata.AddPopularProperty(typeof(Binding), "Path");
        Metadata.AddPopularProperty(typeof(Binding), "Source");
        Metadata.AddPopularProperty(typeof(Binding), "Mode");
        Metadata.AddPopularProperty(typeof(Binding), "RelativeSource");
        Metadata.AddPopularProperty(typeof(Binding), "ElementName");
        Metadata.AddPopularProperty(typeof(Binding), "Converter");

        Metadata.AddPopularProperty(typeof(ItemsControl), "Items");

        // Hide properties that shouldn't be visible in designer
        Metadata.HideProperty(typeof(Visual), "Bounds");
        Metadata.HideProperty(StyledElement.NameProperty);
        Metadata.HideProperty(typeof(Window), "Owner");

        // Popular controls for Avalonia
        Metadata.AddPopularControl(typeof(Button));
        Metadata.AddPopularControl(typeof(Border));
        Metadata.AddPopularControl(typeof(Canvas));
        Metadata.AddPopularControl(typeof(CheckBox));
        Metadata.AddPopularControl(typeof(ComboBox));
        Metadata.AddPopularControl(typeof(DataGrid));
        Metadata.AddPopularControl(typeof(DockPanel));
        Metadata.AddPopularControl(typeof(Expander));
        Metadata.AddPopularControl(typeof(Grid));
        Metadata.AddPopularControl(typeof(Image));
        Metadata.AddPopularControl(typeof(Label));
        Metadata.AddPopularControl(typeof(ListBox));
        Metadata.AddPopularControl(typeof(ProgressBar));
        Metadata.AddPopularControl(typeof(RadioButton));
        Metadata.AddPopularControl(typeof(StackPanel));
        Metadata.AddPopularControl(typeof(ScrollViewer));
        Metadata.AddPopularControl(typeof(Slider));
        Metadata.AddPopularControl(typeof(TabControl));
        Metadata.AddPopularControl(typeof(TextBlock));
        Metadata.AddPopularControl(typeof(TextBox));
        Metadata.AddPopularControl(typeof(TreeView));
        Metadata.AddPopularControl(typeof(Viewbox));
        Metadata.AddPopularControl(typeof(WrapPanel));
        Metadata.AddPopularControl(typeof(Line));
        Metadata.AddPopularControl(typeof(Polyline));
        Metadata.AddPopularControl(typeof(Ellipse));
        Metadata.AddPopularControl(typeof(Rectangle));
        Metadata.AddPopularControl(typeof(Avalonia.Controls.Shapes.Path));

        // Default sizes
        Metadata.AddDefaultSize(typeof(TextBlock), new Size(double.NaN, double.NaN));
        Metadata.AddDefaultSize(typeof(CheckBox), new Size(double.NaN, double.NaN));
        Metadata.AddDefaultSize(typeof(Image), new Size(double.NaN, double.NaN));

        Metadata.AddDefaultSize(typeof(Control), new Size(120, 100));
        Metadata.AddDefaultSize(typeof(ContentControl), new Size(120, 20));
        Metadata.AddDefaultSize(typeof(Button), new Size(75, 23));
        Metadata.AddDefaultSize(typeof(ToggleButton), new Size(75, 23));

        Metadata.AddDefaultSize(typeof(Slider), new Size(120, 20));
        Metadata.AddDefaultSize(typeof(TextBox), new Size(120, 20));
        Metadata.AddDefaultSize(typeof(ComboBox), new Size(120, 20));
        Metadata.AddDefaultSize(typeof(ProgressBar), new Size(120, 20));

        Metadata.AddDefaultSize(typeof(TreeView), new Size(120, 120));
        Metadata.AddDefaultSize(typeof(Label), new Size(130, 120));
        Metadata.AddDefaultSize(typeof(Expander), new Size(130, 120));

        // Default property values
        Metadata.AddDefaultPropertyValue(typeof(Line), Line.StartPointProperty, new Point(0, 0));
        Metadata.AddDefaultPropertyValue(typeof(Line), Line.EndPointProperty, new Point(20, 20));
        Metadata.AddDefaultPropertyValue(typeof(Line), Line.StrokeProperty, Brushes.Black);
        Metadata.AddDefaultPropertyValue(typeof(Line), Line.StrokeThicknessProperty, 2d);
        Metadata.AddDefaultPropertyValue(typeof(Line), Line.StretchProperty, Stretch.None);

        Metadata.AddDefaultPropertyValue(typeof(Polyline), Polyline.PointsProperty,
            new Points { new Point(0, 0), new Point(20, 0), new Point(20, 20) });
        Metadata.AddDefaultPropertyValue(typeof(Polyline), Polyline.StrokeProperty, Brushes.Black);
        Metadata.AddDefaultPropertyValue(typeof(Polyline), Polyline.StrokeThicknessProperty, 2d);
        Metadata.AddDefaultPropertyValue(typeof(Polyline), Polyline.StretchProperty, Stretch.None);

        Metadata.AddDefaultPropertyValue(typeof(Polygon), Polygon.PointsProperty,
            new Points { new Point(0, 20), new Point(20, 20), new Point(10, 0) });
        Metadata.AddDefaultPropertyValue(typeof(Polygon), Polygon.StrokeProperty, Brushes.Black);
        Metadata.AddDefaultPropertyValue(typeof(Polygon), Polygon.StrokeThicknessProperty, 2d);
        Metadata.AddDefaultPropertyValue(typeof(Polygon), Polygon.StretchProperty, Stretch.None);

        Metadata.AddDefaultPropertyValue(typeof(Avalonia.Controls.Shapes.Path), Avalonia.Controls.Shapes.Path.StrokeProperty, Brushes.Black);
        Metadata.AddDefaultPropertyValue(typeof(Avalonia.Controls.Shapes.Path), Avalonia.Controls.Shapes.Path.StrokeThicknessProperty, 2d);
        Metadata.AddDefaultPropertyValue(typeof(Avalonia.Controls.Shapes.Path), Avalonia.Controls.Shapes.Path.StretchProperty, Stretch.None);

        Metadata.AddDefaultPropertyValue(typeof(Rectangle), Rectangle.FillProperty, Brushes.Transparent);
        Metadata.AddDefaultPropertyValue(typeof(Rectangle), Rectangle.StrokeProperty, Brushes.Black);
        Metadata.AddDefaultPropertyValue(typeof(Rectangle), Rectangle.StrokeThicknessProperty, 2d);

        Metadata.AddDefaultPropertyValue(typeof(Ellipse), Ellipse.FillProperty, Brushes.Transparent);
        Metadata.AddDefaultPropertyValue(typeof(Ellipse), Ellipse.StrokeProperty, Brushes.Black);
        Metadata.AddDefaultPropertyValue(typeof(Ellipse), Ellipse.StrokeThicknessProperty, 2d);
    }
}