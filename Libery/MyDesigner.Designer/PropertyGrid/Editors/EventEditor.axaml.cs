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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MyDesigner.Design.PropertyGrid;

namespace MyDesigner.Designer.PropertyGrid.Editors;

/// <summary>
/// Event editor for PropertyGrid - simplified version for Avalonia
/// </summary>
public partial class EventEditor : UserControl
{
  

    public EventEditor()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        txtEventHandler = this.FindControl<TextBox>("txtEventHandler");
        btnNavigate = this.FindControl<Button>("btnNavigate");
        
        if (txtEventHandler != null)
        {
            txtEventHandler.DoubleTapped += TxtEventHandler_DoubleTapped;
            txtEventHandler.KeyDown += TxtEventHandler_KeyDown;
        }
    }

    private void TxtEventHandler_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CreateOrNavigateToHandler();
            e.Handled = true;
        }
    }

    private void TxtEventHandler_DoubleTapped(object sender, TappedEventArgs e)
    {
        CreateOrNavigateToHandler();
    }

    private void NavigateToHandler_Click(object sender, RoutedEventArgs e)
    {
        CreateOrNavigateToHandler();
    }

    private void CreateOrNavigateToHandler()
    {
        try
        {
            var propertyNode = DataContext as PropertyNode;
            if (propertyNode == null)
                return;

            var designItem = propertyNode.FirstProperty?.DesignItem;
            if (designItem == null)
                return;

            // Get event information
            var eventName = propertyNode.Name;
            var controlName = designItem.Name;
            var handlerName = txtEventHandler.Text;

            // If handler is empty, create default name
            if (string.IsNullOrWhiteSpace(handlerName))
            {
                handlerName = string.IsNullOrEmpty(controlName)
                    ? $"{eventName}_Handler"
                    : $"{controlName}_{eventName}";
                
                txtEventHandler.Text = handlerName;
                propertyNode.Value = handlerName;
            }

            // Update XAML with event
            UpdateXamlWithEvent(designItem, eventName, handlerName);

            // TODO: Implement code editor navigation for Avalonia
            // This would require integration with the host application's code editor
            System.Diagnostics.Debug.WriteLine($"[EventEditor] Event handler created: {handlerName} for {eventName}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EventEditor] Error: {ex.Message}");
        }
    }

    private void UpdateXamlWithEvent(DesignItem designItem, string eventName, string handlerName)
    {
        try
        {
            // Add event to DesignItem
            var eventProperty = designItem.Properties.GetProperty(eventName);
            if (eventProperty != null)
            {
                eventProperty.SetValue(handlerName);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EventEditor] Error updating XAML: {ex.Message}");
        }
    }
}