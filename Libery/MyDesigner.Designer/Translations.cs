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

namespace MyDesigner.Designer;

/// <summary>
///     Description of Translations.
/// </summary>
public class Translations
{
    private static Translations _instance;

    public static Translations Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Translations();
            return _instance;
        }
        protected set => _instance = value;
    }

    public virtual string SendToFrontText => "Bring to front";

    public virtual string SendForward => "Forward";

    public virtual string SendBackward => "Backward";

    public virtual string SendToBack => "Send to back";

    public virtual string PressAltText => "Press \"Alt\" to Enter Container";

    public virtual string WrapInCanvas => "Wrap in Canvas";

    public virtual string WrapInGrid => "Wrap in Grid";

    public virtual string WrapInBorder => "Wrap in Border";

    public virtual string WrapInViewbox => "Wrap in Viewbox";

    public virtual string Unwrap => "Unwrap";

    public virtual string FormatedTextEditor => "Formated Text Editor";

    public virtual string ArrangeLeft => "Arrange Left";

    public virtual string ArrangeHorizontalMiddle => "Horizontal centered";

    public virtual string ArrangeRight => "Arrange Right";

    public virtual string ArrangeTop => "Arrange Top";

    public virtual string ArrangeVerticalMiddle => "Vertical centered";

    public virtual string ArrangeBottom => "Arrange Bottom";

    public virtual string EditStyle => "Edit Style";

    public virtual string ComplexEditor => "Complex Editor";
}