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
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using MyDesigner.Design.UIExtensions;

namespace MyDesigner.Designer.MarkupExtensions;

/// <summary>
///     A Binding to a DesignItem of Object
///     This can be used for Example your own Property Pages for Designer Objects
/// </summary>
public class DesignItemBinding : MarkupExtension
{
    private readonly AvaloniaProperty _property;
    private readonly string _propertyName;
    private Binding _binding;
    private DesignItemSetConverter _converter;
    private Control _targetObject;
    private AvaloniaProperty _targetProperty;

    public DesignItemBinding(string path)
    {
        _propertyName = path;

        UpdateSourceTrigger = UpdateSourceTrigger.Default;
        AskWhenMultipleItemsSelected = true;
    }

    public DesignItemBinding(AvaloniaProperty property)
    {
        _property = property;

        UpdateSourceTrigger = UpdateSourceTrigger.Default;
        AskWhenMultipleItemsSelected = true;
    }

    public bool SingleItemProperty { get; set; }

    public bool AskWhenMultipleItemsSelected { get; set; }

    public IValueConverter Converter { get; set; }

    public object ConverterParameter { get; set; }

    public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

    public UpdateSourceTrigger? UpdateSourceTriggerMultipleSelected { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        _targetObject = service?.TargetObject as Control;
        _targetProperty = service?.TargetProperty as AvaloniaProperty;

        if (_targetObject != null) 
        {
            _targetObject.PropertyChanged += targetObject_DataContextChanged;
        }

        return null;
    }

    public void CreateBindingOnProperty(AvaloniaProperty targetProperty, Control targetObject)
    {
        _targetProperty = targetProperty;
        _targetObject = targetObject;
        _targetObject.PropertyChanged += targetObject_DataContextChanged;
        targetObject_DataContextChanged(_targetObject, new AvaloniaPropertyChangedEventArgs<object>(
            _targetObject, StyledElement.DataContextProperty, null, _targetObject.DataContext, BindingPriority.LocalValue));
    }

    private void targetObject_DataContextChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != StyledElement.DataContextProperty) return;
        
        var dcontext = ((Control)sender).DataContext;

        DesignContext context = null;
        Control fe = null;
        DesignItem designItem = null;

        if (dcontext is DesignItem)
        {
            designItem = (DesignItem)dcontext;
            context = designItem.Context;
            fe = designItem.View as Control;
        }
        else if (dcontext is Control)
        {
            fe = (Control)dcontext;
            var srv = fe.FindAncestorOfType<DesignSurface>();
            if (srv != null)
            {
                context = srv.DesignContext;
                designItem = context.Services.Component.GetDesignItem(fe);
            }
        }

        if (context != null)
        {
            if (_property != null)
            {
                _binding = new Binding();
                _binding.Path = _property.Name;
                _binding.Source = fe;
                // In Avalonia, UpdateSourceTrigger is handled differently
                // _binding.UpdateSourceTrigger = UpdateSourceTrigger;

                if (designItem.Services.Selection.SelectedItems.Count > 1 &&
                    UpdateSourceTriggerMultipleSelected != null)
                {
                    // Handle multiple selection case
                }

                _binding.Mode = BindingMode.TwoWay;
                _binding.ConverterParameter = ConverterParameter;

                _converter = new DesignItemSetConverter(designItem, _property, SingleItemProperty,
                    AskWhenMultipleItemsSelected,
                    Converter);
                _binding.Converter = _converter;

                _targetObject.Bind(_targetProperty, _binding);
            }
            else
            {
                _binding = new Binding(_propertyName);
                _binding.Source = fe;
                // In Avalonia, UpdateSourceTrigger is handled differently
                // _binding.UpdateSourceTrigger = UpdateSourceTrigger;

                if (designItem.Services.Selection.SelectedItems.Count > 1 &&
                    UpdateSourceTriggerMultipleSelected != null)
                {
                    // Handle multiple selection case
                }

                _binding.Mode = BindingMode.TwoWay;
                _binding.ConverterParameter = ConverterParameter;

                _converter = new DesignItemSetConverter(designItem, _propertyName, SingleItemProperty,
                    AskWhenMultipleItemsSelected,
                    Converter);
                _binding.Converter = _converter;

                _targetObject.Bind(_targetProperty, _binding);
            }
        }
        else
        {
            _targetObject.ClearValue(_targetProperty);
        }
    }

    private class DesignItemSetConverter : IValueConverter
    {
        private readonly bool _askWhenMultipleItemsSelected;
        private readonly IValueConverter _converter;
        private readonly DesignItem _designItem;
        private readonly AvaloniaProperty _property;
        private readonly string _propertyName;
        private readonly bool _singleItemProperty;

        public DesignItemSetConverter(DesignItem desigItem, string propertyName, bool singleItemProperty,
            bool askWhenMultipleItemsSelected, IValueConverter converter)
        {
            _designItem = desigItem;
            _propertyName = propertyName;
            _singleItemProperty = singleItemProperty;
            _converter = converter;
            _askWhenMultipleItemsSelected = askWhenMultipleItemsSelected;
        }

        public DesignItemSetConverter(DesignItem desigItem, AvaloniaProperty property, bool singleItemProperty,
            bool askWhenMultipleItemsSelected, IValueConverter converter)
        {
            _designItem = desigItem;
            _property = property;
            _propertyName = property.Name;
            _singleItemProperty = singleItemProperty;
            _converter = converter;
            _askWhenMultipleItemsSelected = askWhenMultipleItemsSelected;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_converter != null)
                return _converter.Convert(value, targetType, parameter, culture);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value;
            if (_converter != null)
                val = _converter.ConvertBack(value, targetType, parameter, culture);

            var changeGroup = _designItem.OpenGroup("Property: " + _propertyName);

            try
            {
                DesignItemProperty property = null;

                if (_property != null)
                    try
                    {
                        property = _designItem.Properties.GetProperty(_property);
                    }
                    catch (Exception)
                    {
                        property = _designItem.Properties.GetAttachedProperty(_property);
                    }
                else
                    property = _designItem.Properties.GetProperty(_propertyName);

                property.SetValue(val);

                if (!_singleItemProperty && _designItem.Services.Selection.SelectedItems.Count > 1)
                {
                    var msg = true; // Default to Yes in Avalonia (MessageBox is different)
                    if (_askWhenMultipleItemsSelected)
                    {
                        // In Avalonia, MessageBox works differently
                        // This would need to be implemented with a custom dialog
                        // For now, we'll default to applying to all
                    }
                    if (msg)
                        foreach (var item in _designItem.Services.Selection.SelectedItems)
                        {
                            try
                            {
                                if (_property != null)
                                    property = item.Properties.GetProperty(_property);
                                else
                                    property = item.Properties.GetProperty(_propertyName);
                            }
                            catch (Exception)
                            {
                            }

                            if (property != null)
                                property.SetValue(val);
                        }
                }

                changeGroup.Commit();
            }
            catch (Exception)
            {
                changeGroup.Abort();
            }

            return val;
        }
    }
}