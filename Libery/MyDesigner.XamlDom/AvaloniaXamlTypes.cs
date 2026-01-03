// Avalonia equivalents for System.Xaml types
using System.ComponentModel;
using System.Collections.ObjectModel;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Data;
using Avalonia;
using Avalonia.Metadata;

namespace MyDesigner.XamlDom;

/// <summary>
/// Avalonia equivalent for System.Xaml.XamlType
/// </summary>
public class AvaloniaXamlType
{
    public Type UnderlyingType { get; }
    public string Name => UnderlyingType.Name;
    
    public AvaloniaXamlType(Type type)
    {
        UnderlyingType = type ?? throw new ArgumentNullException(nameof(type));
    }
}

/// <summary>
/// Avalonia equivalent for System.Xaml.XamlMember
/// </summary>
public class AvaloniaXamlMember
{
    public string Name { get; }
    public AvaloniaXamlType DeclaringType { get; }
    public bool IsAttachable { get; }
    
    public AvaloniaXamlMember(string name, AvaloniaXamlType declaringType, bool isAttachable)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
        IsAttachable = isAttachable;
    }
}

/// <summary>
/// Avalonia equivalent for System.Xaml.XamlSchemaContext
/// </summary>
public class AvaloniaXamlSchemaContext
{
    public AvaloniaXamlType GetXamlType(Type type)
    {
        return new AvaloniaXamlType(type);
    }
}

/// <summary>
/// Avalonia equivalent for System.Xaml.XamlSetTypeConverterEventArgs
/// </summary>
public class AvaloniaXamlSetTypeConverterEventArgs : EventArgs
{
    public AvaloniaXamlMember Member { get; }
    public object Value { get; }
    public string CultureInvariantValue { get; }
    public ITypeDescriptorContext TypeDescriptorContext { get; }
    
    public AvaloniaXamlSetTypeConverterEventArgs(AvaloniaXamlMember member, object value, 
        string cultureInvariantValue, ITypeDescriptorContext typeDescriptorContext)
    {
        Member = member;
        Value = value;
        CultureInvariantValue = cultureInvariantValue;
        TypeDescriptorContext = typeDescriptorContext;
    }
}

/// <summary>
/// Avalonia equivalent for System.Xaml.AmbientPropertyValue
/// </summary>
public class AvaloniaAmbientPropertyValue
{
    public AvaloniaXamlMember RetrievedProperty { get; }
    public object Value { get; }
    
    public AvaloniaAmbientPropertyValue(AvaloniaXamlMember retrievedProperty, object value)
    {
        RetrievedProperty = retrievedProperty;
        Value = value;
    }
}

/// <summary>
/// Avalonia equivalent for System.Xaml.IXamlSchemaContextProvider
/// </summary>
public interface IAvaloniaXamlSchemaContextProvider
{
    AvaloniaXamlSchemaContext SchemaContext { get; }
}

/// <summary>
/// Avalonia equivalent for System.Xaml.IAmbientProvider
/// </summary>
public interface IAvaloniaAmbientProvider
{
    AvaloniaAmbientPropertyValue GetFirstAmbientValue(IEnumerable<AvaloniaXamlType> ceilingTypes, params AvaloniaXamlMember[] properties);
    IEnumerable<AvaloniaAmbientPropertyValue> GetAllAmbientValues(IEnumerable<AvaloniaXamlType> ceilingTypes, params AvaloniaXamlMember[] properties);
    IEnumerable<AvaloniaAmbientPropertyValue> GetAllAmbientValues(IEnumerable<AvaloniaXamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<AvaloniaXamlType> types, params AvaloniaXamlMember[] properties);
}

/// <summary>
/// Avalonia equivalent for System.Xaml.IXamlNameResolver
/// </summary>
public interface IAvaloniaXamlNameResolver
{
    object Resolve(string name);
    object Resolve(string name, out bool isFullyInitialized);
    IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope();
    event EventHandler OnNameScopeInitializationComplete;
    bool IsFixupTokenAvailable { get; }
    object GetFixupToken(IEnumerable<string> names);
    object GetFixupToken(IEnumerable<string> names, bool canAssignDirectly);
}

// Missing WPF types - Avalonia equivalents or stubs

/// <summary>
/// Avalonia equivalent for System.Windows.Markup.IMarkupExtension
/// </summary>
public interface IMarkupExtension
{
    object ProvideValue(IServiceProvider serviceProvider);
}

/// <summary>
/// Avalonia equivalent for System.Windows.Markup.IAddChild
/// </summary>
public interface IAddChild
{
    void AddChild(object value);
    void AddText(string text);
}

// /// <summary>
// /// Avalonia equivalent for System.Windows.Markup.ContentPropertyAttribute
// /// </summary>
// [AttributeUsage(AttributeTargets.Property)]
// public sealed class ContentPropertyAttribute : Attribute
// {
//     public string Name { get; }
//     
//     public ContentPropertyAttribute(string name)
//     {
//         Name = name;
//     }
// }

/// <summary>
/// Avalonia equivalent for System.Windows.Markup.RuntimeNamePropertyAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RuntimeNamePropertyAttribute : Attribute
{
    public string Name { get; }
    
    public RuntimeNamePropertyAttribute(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Avalonia equivalent for System.Xaml.XamlSetTypeConverterAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class XamlSetTypeConverterAttribute : Attribute
{
    public string XamlSetTypeConverterHandler { get; }
    
    public XamlSetTypeConverterAttribute(string xamlSetTypeConverterHandler)
    {
        XamlSetTypeConverterHandler = xamlSetTypeConverterHandler;
    }
}


/// <summary>
/// Avalonia equivalent for System.Windows.Markup.NullExtension
/// </summary>
public class NullExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return null;
    }
}

/// <summary>
/// Avalonia equivalent for System.Windows.Data.PriorityBinding
/// </summary>
public class PriorityBinding : MarkupExtension
{
    public Collection<IBinding> Bindings { get; } = new Collection<IBinding>();
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        // Return the first binding that can provide a value
        foreach (var binding in Bindings)
        {
            try
            {
                if (binding is MarkupExtension markupExtension)
                {
                    var value = markupExtension.ProvideValue(serviceProvider);
                    if (value != null) return value;
                }
                else
                {
                    return binding; // Return the binding itself
                }
            }
            catch
            {
                // Continue to next binding
            }
        }
        return null;
    }
}

/// <summary>
/// Avalonia equivalent for System.Windows.Setter
/// </summary>
public class Setter
{
    public AvaloniaProperty Property { get; set; }
    public object Value { get; set; }
    public string TargetName { get; set; }
}

/// <summary>
/// Avalonia equivalent for System.Windows.RoutedEvent
/// </summary>
public class RoutedEvent
{
    public string Name { get; }
    public Type HandlerType { get; }
    public Type OwnerType { get; }
    
    public RoutedEvent(string name, Type handlerType, Type ownerType)
    {
        Name = name;
        HandlerType = handlerType;
        OwnerType = ownerType;
    }
}