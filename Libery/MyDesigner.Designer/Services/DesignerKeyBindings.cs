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

using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Input;

namespace MyDesigner.Designer.Services;

internal class DesignerKeyBindings : IKeyBindingService
{
    private readonly Collection<KeyBinding> _bindings;
    private readonly DesignSurface _surface;

    public DesignerKeyBindings(DesignSurface surface)
    {
        Debug.Assert(surface != null);
        _surface = surface;
        _bindings = new Collection<KeyBinding>();
    }

    public void RegisterBinding(KeyBinding binding)
    {
        if (binding != null)
        {
            _surface.KeyBindings.Add(binding);
            _bindings.Add(binding);
        }
    }

    public void DeregisterBinding(KeyBinding binding)
    {
        if (_bindings.Contains(binding))
        {
            _surface.KeyBindings.Remove(binding);
            _bindings.Remove(binding);
        }
    }

    public KeyBinding GetBinding(KeyGesture gesture)
    {
        return _bindings.FirstOrDefault(binding =>
            binding.Gesture is KeyGesture kg && 
            kg.Key == gesture.Key && 
            kg.KeyModifiers == gesture.KeyModifiers);
    }

    public object Owner => _surface;
}