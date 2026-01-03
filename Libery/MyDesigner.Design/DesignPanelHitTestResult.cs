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

using Avalonia.Media;
using Avalonia;
using MyDesigner.Design.Adorners;

namespace MyDesigner.Design;

/// <summary>
///     Describes the result of a <see cref="IDesignPanel.HitTest(Point, bool, bool, HitTestType)" /> call.
/// </summary>
public struct DesignPanelHitTestResult : IEquatable<DesignPanelHitTestResult>
{
    /// <summary>
    ///     Represents the result that nothing was hit.
    /// </summary>
    public static readonly DesignPanelHitTestResult NoHit = new();

    /// <summary>
    ///     The actual visual that was hit.
    /// </summary>
    public Visual VisualHit { get; }

    /// <summary>
    ///     The adorner panel containing the adorner that was hit.
    /// </summary>
    public AdornerPanel AdornerHit { get; set; }

    /// <summary>
    ///     The model item that was hit.
    /// </summary>
    public DesignItem ModelHit { get; set; }

    /// <summary>
    ///     Create a new DesignPanelHitTestResult instance.
    /// </summary>
    public DesignPanelHitTestResult(Visual visualHit)
    {
        VisualHit = visualHit;
        AdornerHit = null;
        ModelHit = null;
    }

    #region Equals and GetHashCode implementation

    // The code in this region is useful if you want to use this structure in collections.
    // If you don't need it, you can just remove the region and the ": IEquatable<DesignPanelHitTestResult>" declaration.

    /// <summary>
    ///     Tests if this hit test result equals the other result.
    /// </summary>
    public override bool Equals(object obj)
    {
        if (obj is DesignPanelHitTestResult)
            return Equals((DesignPanelHitTestResult)obj); // use Equals method below
        return false;
    }

    /// <summary>
    ///     Tests if this hit test result equals the other result.
    /// </summary>
    public bool Equals(DesignPanelHitTestResult other)
    {
        // add comparisions for all members here
        return VisualHit == other.VisualHit && AdornerHit == other.AdornerHit && ModelHit == other.ModelHit;
    }

    /// <summary>
    ///     Gets the hash code.
    /// </summary>
    public override int GetHashCode()
    {
        // combine the hash codes of all members here (e.g. with XOR operator ^)
        return (VisualHit != null ? VisualHit.GetHashCode() : 0)
               ^ (AdornerHit != null ? AdornerHit.GetHashCode() : 0)
               ^ (ModelHit != null ? ModelHit.GetHashCode() : 0);
    }

    /// <summary />
    public static bool operator ==(DesignPanelHitTestResult lhs, DesignPanelHitTestResult rhs)
    {
        return lhs.Equals(rhs);
    }

    /// <summary />
    public static bool operator !=(DesignPanelHitTestResult lhs, DesignPanelHitTestResult rhs)
    {
        return !lhs.Equals(rhs); // use operator == and negate result
    }

    #endregion
}