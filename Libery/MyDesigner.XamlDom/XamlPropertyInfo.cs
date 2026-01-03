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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace MyDesigner.XamlDom;

/// <summary>
///     Represents a property assignable in XAML.
///     This can be a normal .NET property or an attached property.
/// </summary>
public abstract class XamlPropertyInfo
{
    public abstract TypeConverter TypeConverter { get; }
    public abstract Type TargetType { get; }
    public abstract Type ReturnType { get; }
    public abstract string Name { get; }
    public abstract string FullyQualifiedName { get; }
    public abstract bool IsAttached { get; }
    public abstract bool IsCollection { get; }
    public virtual bool IsEvent => false;
    public virtual bool IsAdvanced => false;
    public virtual AvaloniaProperty DependencyProperty => null;
    public abstract string Category { get; }
    public abstract object GetValue(object instance);
    public abstract void SetValue(object instance, object value);
    public abstract void ResetValue(object instance);
}

#region XamlDependencyPropertyInfo

internal class XamlDependencyPropertyInfo : XamlPropertyInfo
{
    private readonly Func<object, object> attachedGetter;
    private readonly string dependencyPropertyGetterName;
    private readonly bool isAttached;
    private readonly bool isCollection;
    private readonly AvaloniaProperty property;

    public XamlDependencyPropertyInfo(AvaloniaProperty property, bool isAttached, string dependencyPropertyGetterName,
        Func<object, object> attachedGetter = null)
    {
        Debug.Assert(property != null);
        this.property = property;
        this.isAttached = isAttached;
        isCollection = CollectionSupport.IsCollectionType(property.PropertyType);
        this.attachedGetter = attachedGetter;
        this.dependencyPropertyGetterName = dependencyPropertyGetterName;
    }

    public override AvaloniaProperty DependencyProperty => property;

    public override TypeConverter TypeConverter => TypeDescriptor.GetConverter(ReturnType);

    public override string FullyQualifiedName => TargetType.FullName + "." + Name;

    public override Type TargetType => property.OwnerType;

    public override Type ReturnType => property.PropertyType;

    public override string Name => dependencyPropertyGetterName;

    public override string Category => "Misc";

    public override bool IsAttached => isAttached;

    public override bool IsCollection => isCollection;

    public override object GetValue(object instance)
    {
        try
        {
            if (attachedGetter != null) return attachedGetter(instance);
        }
        catch (Exception)
        {
        }

        var avaloniaObject = instance as AvaloniaObject;
        if (avaloniaObject != null) return avaloniaObject.GetValue(property);

        return null;
    }

    public override void SetValue(object instance, object value)
    {
        ((AvaloniaObject)instance).SetValue(property, value);
    }

    public override void ResetValue(object instance)
    {
        ((AvaloniaObject)instance).ClearValue(property);
    }
}

#endregion

#region XamlNormalPropertyInfo

internal sealed class XamlNormalPropertyInfo : XamlPropertyInfo
{
    public static readonly TypeConverter StringTypeConverter = TypeDescriptor.GetConverter(typeof(string));
    private readonly PropertyDescriptor _propertyDescriptor;

    public XamlNormalPropertyInfo(PropertyDescriptor propertyDescriptor)
    {
        _propertyDescriptor = propertyDescriptor;
        // Try to find corresponding AvaloniaProperty
        var avaloniaPropertyField = propertyDescriptor.ComponentType.GetField(propertyDescriptor.Name + "Property", 
            BindingFlags.Public | BindingFlags.Static);
        if (avaloniaPropertyField != null && typeof(AvaloniaProperty).IsAssignableFrom(avaloniaPropertyField.FieldType))
        {
            DependencyProperty = (AvaloniaProperty)avaloniaPropertyField.GetValue(null);
        }
    }

    public override AvaloniaProperty DependencyProperty { get; }

    public override Type ReturnType => _propertyDescriptor.PropertyType;

    public override Type TargetType => _propertyDescriptor.ComponentType;

    public override string Category => _propertyDescriptor.Category;

    public override TypeConverter TypeConverter =>
        GetCustomTypeConverter(_propertyDescriptor.PropertyType) ?? _propertyDescriptor.Converter;

    public override string FullyQualifiedName =>
        _propertyDescriptor.ComponentType.FullName + "." + _propertyDescriptor.Name;

    public override string Name => _propertyDescriptor.Name;

    public override bool IsAttached => false;

    public override bool IsCollection => CollectionSupport.IsCollectionType(_propertyDescriptor.PropertyType);

    public override bool IsAdvanced
    {
        get
        {
            var a = _propertyDescriptor.Attributes[typeof(EditorBrowsableAttribute)] as EditorBrowsableAttribute;
            if (a != null) return a.State == EditorBrowsableState.Advanced;
            return false;
        }
    }

    public override object GetValue(object instance)
    {
        return _propertyDescriptor.GetValue(instance);
    }

    public override void SetValue(object instance, object value)
    {
        _propertyDescriptor.SetValue(instance, value);
    }

    public override void ResetValue(object instance)
    {
        try
        {
            _propertyDescriptor.ResetValue(instance);
        }
        catch (Exception)
        {
            //For Example "UndoRedoSimpleBinding" will raise a exception here => look if it has Side Effects if we generally catch here?
        }
    }

    public static TypeConverter GetCustomTypeConverter(Type propertyType)
    {
        if (propertyType == typeof(object))
            return StringTypeConverter;
        if (propertyType == typeof(Type))
            return TypeTypeConverter.Instance;
        if (typeof(AvaloniaProperty).IsAssignableFrom(propertyType))
            return DependencyPropertyConverter.Instance;
        return null;
    }

    private sealed class TypeTypeConverter : TypeConverter
    {
        public static readonly TypeTypeConverter Instance = new();

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                return null;
            if (value is string)
            {
                var xamlTypeResolver = (IXamlTypeResolver)context.GetService(typeof(IXamlTypeResolver));
                if (xamlTypeResolver == null)
                    throw new XamlLoadException("IXamlTypeResolver not found in type descriptor context.");
                return xamlTypeResolver.Resolve((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    private sealed class DependencyPropertyConverter : TypeConverter
    {
        public static readonly DependencyPropertyConverter Instance = new();

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                return null;
            if (value is string)
            {
                var xamlTypeResolver = (XamlTypeResolverProvider)context.GetService(typeof(XamlTypeResolverProvider));
                if (xamlTypeResolver == null)
                    throw new XamlLoadException("XamlTypeResolverProvider not found in type descriptor context.");
                var prop = xamlTypeResolver.ResolveProperty((string)value);
                if (prop == null)
                    throw new XamlLoadException("Could not find property " + value + ".");
                var depProp = prop as XamlDependencyPropertyInfo;
                if (depProp != null)
                    return depProp.DependencyProperty;
                var field = prop.TargetType.GetField(prop.Name + "Property", BindingFlags.Public | BindingFlags.Static);
                if (field != null && typeof(AvaloniaProperty).IsAssignableFrom(field.FieldType))
                    return (AvaloniaProperty)field.GetValue(null);
                throw new XamlLoadException("Property " + value + " is not an avalonia property.");
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}

#endregion

#region XamlEventPropertyInfo

internal sealed class XamlEventPropertyInfo : XamlPropertyInfo
{
    private readonly EventDescriptor _eventDescriptor;

    public XamlEventPropertyInfo(EventDescriptor eventDescriptor)
    {
        _eventDescriptor = eventDescriptor;
    }

    public override Type ReturnType => _eventDescriptor.EventType;

    public override Type TargetType => _eventDescriptor.ComponentType;

    public override string Category => _eventDescriptor.Category;

    public override TypeConverter TypeConverter => XamlNormalPropertyInfo.StringTypeConverter;

    public override string FullyQualifiedName => _eventDescriptor.ComponentType.FullName + "." + _eventDescriptor.Name;

    public override string Name => _eventDescriptor.Name;

    public override bool IsEvent => true;

    public override bool IsAttached => false;

    public override bool IsCollection => false;

    public override object GetValue(object instance)
    {
        throw new NotSupportedException();
    }

    public override void SetValue(object instance, object value)
    {
    }

    public override void ResetValue(object instance)
    {
    }
}

#endregion