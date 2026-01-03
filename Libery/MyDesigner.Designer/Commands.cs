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


using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;



namespace MyDesigner.Designer;

/// <summary>
///     Description of Commands.
/// </summary>
public static class Commands
{
    public static RelayCommand AlignTopCommand = new RelayCommand(() => { });
    public static RelayCommand AlignMiddleCommand = new RelayCommand(() => { });
    public static RelayCommand AlignBottomCommand = new RelayCommand(() => { });
    public static RelayCommand AlignLeftCommand = new RelayCommand(() => { });
    public static RelayCommand AlignCenterCommand = new RelayCommand(() => { });
    public static RelayCommand AlignRightCommand = new RelayCommand(() => { });
    public static RelayCommand RotateLeftCommand = new RelayCommand(() => { });
    public static RelayCommand RotateRightCommand = new RelayCommand(() => { });
    public static RelayCommand StretchToSameWidthCommand = new RelayCommand(() => { });
    public static RelayCommand StretchToSameHeightCommand = new RelayCommand(() => { });
    public static RelayCommand GridSettingsCommand = new RelayCommand(() => { });

    static Commands()
    {
    }
}

/// <summary>
/// ApplicationCommands replacement using CommunityToolkit.Mvvm
/// </summary>
public static class ApplicationCommands
{
    // Create RelayCommand equivalents for ApplicationCommands
    public static RelayCommand Undo { get; } = new RelayCommand(() => { /* TODO: Implement */ });
    public static RelayCommand Redo { get; } = new RelayCommand(() => { /* TODO: Implement */ });
    public static RelayCommand Copy { get; } = new RelayCommand(() => { /* TODO: Implement */ });
    public static RelayCommand Cut { get; } = new RelayCommand(() => { /* TODO: Implement */ });
    public static RelayCommand Delete { get; } = new RelayCommand(() => { /* TODO: Implement */ });
    public static RelayCommand Paste { get; } = new RelayCommand(() => { /* TODO: Implement */ });
    public static RelayCommand SelectAll { get; } = new RelayCommand(() => { /* TODO: Implement */ });
}