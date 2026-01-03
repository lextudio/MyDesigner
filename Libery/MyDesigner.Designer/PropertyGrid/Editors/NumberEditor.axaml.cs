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
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MyDesigner.Design.PropertyGrid;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.PropertyGrid.Editors;

[TypeEditor(typeof(byte))]
[TypeEditor(typeof(sbyte))]
[TypeEditor(typeof(decimal))]
[TypeEditor(typeof(double))]
[TypeEditor(typeof(float))]
[TypeEditor(typeof(int))]
[TypeEditor(typeof(uint))]
[TypeEditor(typeof(long))]
[TypeEditor(typeof(ulong))]
[TypeEditor(typeof(short))]
[TypeEditor(typeof(ushort))]
[TypeEditor(typeof(byte?))]
[TypeEditor(typeof(sbyte?))]
[TypeEditor(typeof(decimal?))]
[TypeEditor(typeof(double?))]
[TypeEditor(typeof(float?))]
[TypeEditor(typeof(int?))]
[TypeEditor(typeof(uint?))]
[TypeEditor(typeof(long?))]
[TypeEditor(typeof(ulong?))]
[TypeEditor(typeof(short?))]
[TypeEditor(typeof(ushort?))]
public partial class NumberEditor : Controls.NumericUpDown
{
    private static readonly Dictionary<Type, double> minimums = new();
    private static readonly Dictionary<Type, double> maximums = new();

    private ChangeGroup group;

    static NumberEditor()
    {
        minimums[typeof(byte)] = byte.MinValue;
        minimums[typeof(sbyte)] = sbyte.MinValue;
        minimums[typeof(decimal)] = (double)decimal.MinValue;
        minimums[typeof(double)] = double.MinValue;
        minimums[typeof(float)] = float.MinValue;
        minimums[typeof(int)] = int.MinValue;
        minimums[typeof(uint)] = uint.MinValue;
        minimums[typeof(long)] = long.MinValue;
        minimums[typeof(ulong)] = ulong.MinValue;
        minimums[typeof(short)] = short.MinValue;
        minimums[typeof(ushort)] = ushort.MinValue;

        maximums[typeof(byte)] = byte.MaxValue;
        maximums[typeof(sbyte)] = sbyte.MaxValue;
        maximums[typeof(decimal)] = (double)decimal.MaxValue;
        maximums[typeof(double)] = double.MaxValue;
        maximums[typeof(float)] = float.MaxValue;
        maximums[typeof(int)] = int.MaxValue;
        maximums[typeof(uint)] = uint.MaxValue;
        maximums[typeof(long)] = long.MaxValue;
        maximums[typeof(ulong)] = ulong.MaxValue;
        maximums[typeof(short)] = short.MaxValue;
        maximums[typeof(ushort)] = ushort.MaxValue;
    }

    public NumberEditor()
    {
        InitializeComponent();
        DataContextChanged += NumberEditor_DataContextChanged;
    }

    public PropertyNode PropertyNode => DataContext as PropertyNode;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void NumberEditor_DataContextChanged(object sender, EventArgs e)
    {
        if (PropertyNode == null) return;
        var type = PropertyNode.FirstProperty.ReturnType;

        var range = Metadata.GetValueRange(PropertyNode.FirstProperty);
        if (range == null) range = new NumberRange { Min = double.MinValue, Max = double.MaxValue };

        var nType = type;
        if (Nullable.GetUnderlyingType(type) != null) nType = Nullable.GetUnderlyingType(type);

        if (range.Min == double.MinValue)
            Minimum = minimums[nType];
        else
            Minimum = range.Min;

        if (range.Max == double.MaxValue)
            Maximum = maximums[nType];
        else
            Maximum = range.Max;

        if (type == typeof(double) || type == typeof(decimal)) 
            DecimalPlaces = 2;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        if (textBox != null)
            textBox.TextChanged += TextValueChanged;
    }

    private void TextValueChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (PropertyNode == null)
            return;
        if (textBox == null)
            return;
        double val;

        if (double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
        {
            if (IsValidTypeConverter(PropertyNode.FirstProperty.TypeConverter, textBox.Text))
            {
                if ((val >= Minimum && val <= Maximum) || double.IsNaN(val))
                {
                    textBox.Foreground = Brushes.Black;
                    ToolTip.SetTip(textBox, textBox.Text);
                }
                else
                {
                    textBox.Foreground = Brushes.DarkBlue;
                    ToolTip.SetTip(textBox, "Value should be in between " + Minimum + " and " + Maximum);
                }
            }
            else
            {
                textBox.Foreground = Brushes.DarkRed;
                ToolTip.SetTip(textBox, "Cannot convert to Type : " + PropertyNode.FirstProperty.ReturnType.Name);
            }
        }
        else
        {
            textBox.Foreground = Brushes.DarkRed;
            ToolTip.SetTip(textBox, string.IsNullOrWhiteSpace(textBox.Text)
                ? null
                : "Value does not belong to any numeric type");
        }
    }

    // Method used instead of System.ComponentModel.TypeConverter.IsValid()
    // This ensures that TypeConverter is validated based on the current culture
    // See: https://stackoverflow.com/questions/16837774/typeconverter-isvalid-uses-current-thread-culture-but-typeconverter-convertfro
    private static bool IsValidTypeConverter(TypeConverter typeConverter, object value)
    {
        var isValid = true;
        try
        {
            if (value == null || typeConverter.CanConvertFrom(value.GetType()))
                typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
            else
                isValid = false;
        }
        catch
        {
            isValid = false;
        }

        return isValid;
    }
}