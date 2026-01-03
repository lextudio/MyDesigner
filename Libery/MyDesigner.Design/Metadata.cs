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

using System.Collections;
using System.Reflection;
using Avalonia;

namespace MyDesigner.Design;

/// <summary>
///     Contains helper methods for retrieving meta data.
/// </summary>
public static class Metadata
{
    // TODO: do we really want to store these values in a static dictionary?
    // Why not per-design context (as a service?)
    private static readonly Dictionary<Type, List<object>> standardValues = new();
    private static readonly Dictionary<AvaloniaProperty, object[]> standardValuesForAvaloniaPropertys = new();
    private static readonly Dictionary<Type, List<NamedValue>> standardNamedValues = new();
    private static readonly Dictionary<Type, Dictionary<AvaloniaProperty, object>> standardPropertyValues = new();

    private static readonly HashSet<string> hiddenProperties = new();
    private static readonly HashSet<string> popularProperties = new();
    private static readonly HashSet<Type> popularControls = new();
    private static readonly Dictionary<string, NumberRange> ranges = new();
    private static readonly HashSet<Type> placementDisabled = new();
    private static readonly Dictionary<Type, Size> defaultSizes = new();

    /// <summary>
    ///     Gets the full name of an avalonia property (OwnerType.FullName + "." + Name).
    /// </summary>
    public static string GetFullName(this AvaloniaProperty p)
    {
        return p.OwnerType.FullName + "." + p.Name;
    }

    /// <summary>
    ///     Registers a set of standard values for a <paramref name="type" /> by using the
    ///     public static properties of the type <paramref name="valuesContainer" />.
    /// </summary>
    /// <example>Metadata.AddStandardValues(typeof(Brush), typeof(Brushes));</example>
    public static void AddStandardValues(Type type, Type valuesContainer)
    {
        AddStandardValues(type,
            valuesContainer.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Select(p => p.GetValue(null, null)));
    }

    /// <summary>
    ///     Registers a set of standard values for a <paramref name="avaloniaProperty" /> by using the
    ///     <paramref name="values" />.
    /// </summary>
    /// <example>Metadata.AddStandardValues(typeof(Brush), typeof(Brushes));</example>
    public static void AddStandardValues(AvaloniaProperty avaloniaProperty, params object[] values)
    {
        lock (standardValuesForAvaloniaPropertys)
        {
            standardValuesForAvaloniaPropertys[avaloniaProperty] = values;
        }
    }

    /// <summary>
    ///     Registers a set of standard values for a <paramref name="type" /> by using the
    ///     public static properties of the type <paramref name="valuesContainer" />.
    /// </summary>
    /// <example>Metadata.AddDoubleNamedStandardValues(typeof(Brush), typeof(Brushes));</example>
    public static void AddDoubleNamedStandardValues(Type type, Type valuesContainer)
    {
        List<NamedValue> list;

        var pList = valuesContainer.GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Select(p => new NamedValue { Name = p.Name, Value = p.GetValue(null, null) });

        lock (standardNamedValues)
        {
            if (!standardNamedValues.TryGetValue(type, out list))
            {
                list = new List<NamedValue>();
                standardNamedValues[type] = list;
            }

            foreach (var v in pList) list.Add(v);
        }
    }

    /// <summary>
    ///     Registers a set of standard <paramref name="values" /> for a <paramref name="type" />.
    /// </summary>
    /// <remarks>You can call this method multiple times to add additional standard values.</remarks>
    public static void AddStandardValues<T>(Type type, IEnumerable<T> values)
    {
        List<object> list;
        lock (standardValues)
        {
            if (!standardValues.TryGetValue(type, out list))
            {
                list = new List<object>();
                standardValues[type] = list;
            }

            foreach (var v in values) list.Add(v);
        }
    }

    /// <summary>
    ///     Retrieves the standard values for the specified <paramref name="type" />.
    /// </summary>
    public static IEnumerable GetNamedStandardValues(Type type)
    {
        List<NamedValue> values;
        lock (standardNamedValues)
        {
            if (standardNamedValues.TryGetValue(type, out values)) return values;
        }

        return null;
    }

    /// <summary>
    ///     Retrieves the standard values for the specified <paramref name="type" />.
    /// </summary>
    public static IEnumerable GetStandardValues(Type type)
    {
        var baseT = Nullable.GetUnderlyingType(type);

        if (type.IsEnum) return Enum.GetValues(type);

        if (baseT != null && baseT.IsEnum) return Enum.GetValues(baseT);

        List<object> values;
        lock (standardValues)
        {
            if (standardValues.TryGetValue(type, out values)) return values;
        }

        return null;
    }

    /// <summary>
    ///     Retrieves the standard values for the specified <paramref name="avaloniaProperty" />.
    /// </summary>
    public static IEnumerable GetStandardValues(AvaloniaProperty avaloniaProperty)
    {
        object[] values;
        lock (standardValuesForAvaloniaPropertys)
        {
            if (standardValuesForAvaloniaPropertys.TryGetValue(avaloniaProperty, out values)) return values;
        }

        return null;
    }

    /// <summary>
    ///     Hides the specified property (marks it as not browsable).
    /// </summary>
    public static void HideProperty(AvaloniaProperty p)
    {
        lock (hiddenProperties)
        {
            hiddenProperties.Add(p.GetFullName());
        }
    }

    /// <summary>
    ///     Hides the specified property (marks it as not browsable).
    /// </summary>
    public static void HideProperty(Type type, string member)
    {
        lock (hiddenProperties)
        {
            hiddenProperties.Add(type.FullName + "." + member);
        }
    }

    /// <summary>
    ///     Gets whether the specified property is browsable (should be visible in property grids).
    /// </summary>
    public static bool IsBrowsable(DesignItemProperty p)
    {
        lock (hiddenProperties)
        {
            if (hiddenProperties.Contains(p.AvaloniaFullName)) return false;
        }

        return true;
    }

    /// <summary>
    ///     Registers a popular property (shown first in the property grid).
    /// </summary>
    public static void AddPopularProperty(AvaloniaProperty p)
    {
        lock (popularProperties)
        {
            popularProperties.Add(p.GetFullName());
        }
    }

    /// <summary>
    ///     Registers a popular property (shown first in the property grid).
    /// </summary>
    public static void AddPopularProperty(Type type, string member)
    {
        lock (popularProperties)
        {
            popularProperties.Add(type.FullName + "." + member);
        }
    }

    /// <summary>
    ///     Gets whether the specified property was registered as popular.
    /// </summary>
    public static bool IsPopularProperty(DesignItemProperty p)
    {
        lock (popularProperties)
        {
            if (popularProperties.Contains(p.AvaloniaFullName)) return true;
        }

        return false;
    }

    /// <summary>
    ///     Registers a popular control (visible in the default toolbox).
    /// </summary>
    public static void AddPopularControl(Type t)
    {
        lock (popularControls)
        {
            popularControls.Add(t);
        }
    }

    /// <summary>
    ///     Gets the list of popular controls.
    /// </summary>
    public static IEnumerable<Type> GetPopularControls()
    {
        lock (popularControls)
        {
            return popularControls.ToArray();
        }
    }

    /// <summary>
    ///     Gets whether the specified control was registered as popular.
    /// </summary>
    public static bool IsPopularControl(Type t)
    {
        lock (popularControls)
        {
            return popularControls.Contains(t);
        }
    }

    /// <summary>
    ///     Registers the value range for the property.
    /// </summary>
    public static void AddValueRange(AvaloniaProperty p, double min, double max)
    {
        lock (ranges)
        {
            ranges[p.GetFullName()] = new NumberRange { Min = min, Max = max };
        }
    }

    /// <summary>
    ///     Gets the registered value range for the property, or null if no range was registered.
    /// </summary>
    public static NumberRange GetValueRange(DesignItemProperty p)
    {
        NumberRange r;
        lock (ranges)
        {
            if (ranges.TryGetValue(p.AvaloniaFullName, out r)) return r;
        }

        return null;
    }

    /// <summary>
    ///     Disables the default placement behaviour (setting the ContentProperty) for the type.
    /// </summary>
    public static void DisablePlacement(Type type)
    {
        lock (placementDisabled)
        {
            placementDisabled.Add(type);
        }
    }

    /// <summary>
    ///     Gets whether thr default placement behaviour (setting the ContentProperty) was disabled for the type.
    /// </summary>
    public static bool IsPlacementDisabled(Type type)
    {
        lock (placementDisabled)
        {
            return placementDisabled.Contains(type);
        }
    }

    /// <summary>
    ///     Registers a default size for new controls of the specified type.
    /// </summary>
    public static void AddDefaultSize(Type t, Size s)
    {
        lock (defaultSizes)
        {
            defaultSizes[t] = s;
        }
    }

    /// <summary>
    ///     Gets the default size for new controls of the specified type,
    ///     or new Size(double.NaN, double.NaN) if no default size was registered.
    /// </summary>
    public static Size? GetDefaultSize(Type t, bool checkBasetype = true)
    {
        Size s;
        lock (defaultSizes)
        {
            while (t != null)
            {
                if (defaultSizes.TryGetValue(t, out s)) return s;
                t = checkBasetype ? t.BaseType : null;
            }
        }

        return null;
    }

    /// <summary>
    ///     Registers a default Property Value wich should be used
    /// </summary>
    public static void AddDefaultPropertyValue(Type t, AvaloniaProperty p, object value)
    {
        lock (standardPropertyValues)
        {
            if (!standardPropertyValues.ContainsKey(t))
                standardPropertyValues.Add(t, new Dictionary<AvaloniaProperty, object>());

            standardPropertyValues[t][p] = value;
        }
    }

    /// <summary>
    ///     Gets Default Propertie Values for a type
    /// </summary>
    public static Dictionary<AvaloniaProperty, object> GetDefaultPropertyValues(Type t)
    {
        lock (standardPropertyValues)
        {
            if (standardPropertyValues.ContainsKey(t))
                return standardPropertyValues[t];

            return null;
        }
    }

    private class NamedValue
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }
}

/// <summary>
///     Represets the minimum and maximum valid value for a double property.
/// </summary>
public class NumberRange
{
    /// <summary>
    ///     Gets/Sets the minimum value.
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    ///     Gets/Sets the maximum value.
    /// </summary>
    public double Max { get; set; }
}