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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MyDesigner.Design.Interfaces;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.OutlineView;

public partial class Outline : UserControl
{
    public static readonly StyledProperty<IOutlineNode> RootProperty =
        AvaloniaProperty.Register<Outline, IOutlineNode>(nameof(Root));

    public Outline()
    {
        InitializeComponent();

        // In Avalonia, command handling is different
        // These would need to be implemented using Avalonia's command system
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.Z, KeyModifiers.Control), Command = new RelayCommand(() => UndoCommand()) });
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.Y, KeyModifiers.Control), Command = new RelayCommand(() => RedoCommand()) });
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.C, KeyModifiers.Control), Command = new RelayCommand(() => CopyCommand()) });
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.X, KeyModifiers.Control), Command = new RelayCommand(() => CutCommand()) });
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.Delete), Command = new RelayCommand(() => DeleteCommand()) });
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.V, KeyModifiers.Control), Command = new RelayCommand(() => PasteCommand()) });
        KeyBindings.Add(new KeyBinding { Gesture = new KeyGesture(Key.A, KeyModifiers.Control), Command = new RelayCommand(() => SelectAllCommand()) });
    }

    public IOutlineNode Root
    {
        get => GetValue(RootProperty);
        set => SetValue(RootProperty, value);
    }

    public object OutlineContent => this;

    private void UndoCommand()
    {
        if (Root != null)
            ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Undo();
    }

    private void RedoCommand()
    {
        if (Root != null)
            ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Redo();
    }

    private void CopyCommand()
    {
        if (Root != null)
            ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Copy();
    }

    private void CutCommand()
    {
        if (Root != null)
            ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Cut();
    }

    private void DeleteCommand()
    {
        if (Root != null)
            ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Delete();
    }

    private void PasteCommand()
    {
        if (Root != null)
            ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Paste();
    }

    private void SelectAllCommand()
    {
        if (Root != null)
            ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.SelectAll();
    }
}