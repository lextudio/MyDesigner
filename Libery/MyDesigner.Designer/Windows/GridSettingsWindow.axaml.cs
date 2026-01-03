using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace MyDesigner.Designer.Windows;

public partial class GridSettingsWindow : Window
{
    private DesignPanel _designPanel;

    public GridSettingsWindow(DesignPanel designPanel)
    {
        InitializeComponent();
        _designPanel = designPanel;
        
        // تحميل الإعدادات الحالية
        LoadCurrentSettings();
        
        // تحديث المعاينة عند تغيير القيم
        GridSizeSlider.PropertyChanged += (s, e) => 
        {
            if (e.Property == Slider.ValueProperty)
                UpdatePreview();
        };
        ShowGridCheckBox.PropertyChanged += (s, e) => 
        {
            if (e.Property == CheckBox.IsCheckedProperty)
                UpdatePreview();
        };
        
        // عرض المعاينة الأولية
        UpdatePreview();
    }

    private void LoadCurrentSettings()
    {
        if (_designPanel != null)
        {
            // تفعيل الشبكة تلقائياً عند فتح النافذة
            if (!_designPanel.UseRasterPlacement)
            {
                _designPanel.UseRasterPlacement = true;
            }
            ShowGridCheckBox.IsChecked = _designPanel.UseRasterPlacement;
            GridSizeSlider.Value = _designPanel.RasterWidth;
        }
    }

    private void UpdatePreview()
    {
        if (PreviewBorder == null) return;
        
        if (ShowGridCheckBox.IsChecked == true)
        {
            double gridSize = GridSizeSlider.Value;
            double largeGridSize = gridSize * 4;
            
            var drawingGroup = new DrawingGroup();
            
            // رسم الخلفية
            drawingGroup.Children.Add(new GeometryDrawing
            {
                Brush = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                Geometry = new RectangleGeometry(new Rect(0, 0, largeGridSize, largeGridSize))
            });
            
            // رسم الشبكة الرئيسية فقط
            var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(80, 150, 150, 150)), 1.0);
            drawingGroup.Children.Add(new GeometryDrawing
            {
                Pen = gridPen,
                Geometry = new LineGeometry(new Point(0, 0), new Point(0, largeGridSize))
            });
            drawingGroup.Children.Add(new GeometryDrawing
            {
                Pen = gridPen,
                Geometry = new LineGeometry(new Point(0, 0), new Point(largeGridSize, 0))
            });
            
            var drawingBrush = new DrawingBrush(drawingGroup)
            {
                TileMode = TileMode.Tile,
                DestinationRect = new RelativeRect(0, 0, largeGridSize, largeGridSize, RelativeUnit.Absolute)
            };
            
            PreviewBorder.Background = drawingBrush;
        }
        else
        {
            PreviewBorder.Background = Brushes.White;
        }
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (_designPanel != null)
        {
            _designPanel.RasterWidth = (int)GridSizeSlider.Value;
            _designPanel.UseRasterPlacement = ShowGridCheckBox.IsChecked == true;
        }
        
        Close(true);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}