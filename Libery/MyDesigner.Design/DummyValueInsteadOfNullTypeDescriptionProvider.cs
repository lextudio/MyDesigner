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
using System.ComponentModel;

namespace MyDesigner.Design;

/// <summary>
///     Description of DummyValueInsteadOfNullTypeDescriptionProvider.
/// </summary>
public sealed class DummyValueInsteadOfNullTypeDescriptionProvider : TypeDescriptionProvider
{
    private readonly object _dummyValue;
    // By using a TypeDescriptionProvider, we can intercept all access to the property that is
    // using a PropertyDescriptor. AvaloniaDesign uses a PropertyDescriptor for accessing
    // properties (except for attached properties), so even DesignItemProperty/AvaloniaProperty.ValueOnInstance
    // will report null when the actual value is the dummy value.

    private readonly string _propertyName;

    /// <summary>
    ///     Initializes a new instance of <see cref="DummyValueInsteadOfNullTypeDescriptionProvider" />.
    /// </summary>
    public DummyValueInsteadOfNullTypeDescriptionProvider(TypeDescriptionProvider existingProvider,
        string propertyName, object dummyValue)
        : base(existingProvider)
    {
        _propertyName = propertyName;
        _dummyValue = dummyValue;
    }

    /// <inheritdoc />
    public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
    {
        return new ShadowTypeDescriptor(this, base.GetTypeDescriptor(objectType, instance));
    }

    private sealed class ShadowTypeDescriptor : CustomTypeDescriptor
    {
        private readonly DummyValueInsteadOfNullTypeDescriptionProvider _parent;

        public ShadowTypeDescriptor(DummyValueInsteadOfNullTypeDescriptionProvider parent,
            ICustomTypeDescriptor existingDescriptor)
            : base(existingDescriptor)
        {
            _parent = parent;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return Filter(base.GetProperties());
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return Filter(base.GetProperties(attributes));
        }

        private PropertyDescriptorCollection Filter(PropertyDescriptorCollection properties)
        {
            var property = properties[_parent._propertyName];
            if (property != null)
            {
                if ((properties as IDictionary).IsReadOnly)
                    properties = new PropertyDescriptorCollection(properties.Cast<PropertyDescriptor>().ToArray());
                properties.Remove(property);
                properties.Add(new ShadowPropertyDescriptor(_parent, property));
            }

            return properties;
        }
    }

    private sealed class ShadowPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _baseDescriptor;
        private readonly DummyValueInsteadOfNullTypeDescriptionProvider _parent;

        public ShadowPropertyDescriptor(DummyValueInsteadOfNullTypeDescriptionProvider parent,
            PropertyDescriptor existingDescriptor)
            : base(existingDescriptor)
        {
            _parent = parent;
            _baseDescriptor = existingDescriptor;
        }

        public override Type ComponentType => _baseDescriptor.ComponentType;

        public override bool IsReadOnly => _baseDescriptor.IsReadOnly;

        public override Type PropertyType => _baseDescriptor.PropertyType;

        public override bool CanResetValue(object component)
        {
            return _baseDescriptor.CanResetValue(component);
        }

        public override object GetValue(object component)
        {
            var value = _baseDescriptor.GetValue(component);
            if (value == _parent._dummyValue)
                return null;
            return value;
        }

        public override void ResetValue(object component)
        {
            _baseDescriptor.SetValue(component, _parent._dummyValue);
        }

        public override void SetValue(object component, object value)
        {
            //_baseDescriptor.SetValue(component, value ?? _parent._dummyValue);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return _baseDescriptor.ShouldSerializeValue(component)
                   && _baseDescriptor.GetValue(component) != _parent._dummyValue;
        }
    }
}