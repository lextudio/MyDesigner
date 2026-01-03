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
using Avalonia.Media;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;

namespace MyDesigner.Designer.Extensions;

[ExtensionFor(typeof(Avalonia.Controls.Image))]
public class BorderForImageControl : PermanentAdornerProvider
{
    private AdornerPanel adornerPanel;
    private Border border;
    private AdornerPanel cachedAdornerPanel;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ExtendedItem.PropertyChanged += OnPropertyChanged;

        UpdateAdorner();
    }

    protected override void OnRemove()
    {
        ExtendedItem.PropertyChanged -= OnPropertyChanged;
        base.OnRemove();
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender == null || e.PropertyName == "Width" || e.PropertyName == "Height")
            ((DesignPanel)ExtendedItem.Services.DesignPanel).AdornerLayer.UpdateAdornersForElement(ExtendedItem.View,
                true);
    }

    private void UpdateAdorner()
    {
        var element = ExtendedItem.Component as Visual;
        if (element != null) CreateAdorner();
    }

    private void CreateAdorner()
    {
        if (adornerPanel == null)
        {
            if (cachedAdornerPanel == null)
            {
                cachedAdornerPanel = new AdornerPanel();
                cachedAdornerPanel.Order = AdornerOrder.Background;
                border = new Border();
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
                border.Background = Brushes.Transparent;
                border.MinWidth = 1;
                border.MinHeight = 1;

                border.PointerPressed += border_PointerPressed;

                AdornerPanel.SetPlacement(border, AdornerPlacement.FillContent);
                cachedAdornerPanel.Children.Add(border);
            }

            adornerPanel = cachedAdornerPanel;
            Adorners.Add(adornerPanel);
        }
    }

    private void border_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var keyModifiers = e.KeyModifiers;
        if (!keyModifiers.HasFlag(KeyModifiers.Alt) && ((Image)ExtendedItem.View).Source == null)
        {
            e.Handled = true;
            ExtendedItem.Services.Selection.SetSelectedComponents(new[] { ExtendedItem },
                SelectionTypes.Auto);
            ((DesignPanel)ExtendedItem.Services.DesignPanel).Focus();
        }
    }
}