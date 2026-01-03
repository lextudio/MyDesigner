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

namespace MyDesigner.Design.Adorners;

/// <summary>
///     Describes where an Adorner is positioned on the Z-Layer.
/// </summary>
public struct AdornerOrder : IComparable<AdornerOrder>, IEquatable<AdornerOrder>
{
    /// <summary>
    ///     The adorner is in the background layer.
    /// </summary>
    public static readonly AdornerOrder Background = new(100);

    /// <summary>
    ///     The adorner is in the content layer.
    /// </summary>
    public static readonly AdornerOrder Content = new(200);

    /// <summary>
    ///     The adorner is in the layer behind the foreground but above the content. This layer
    ///     is used for the gray-out effect.
    /// </summary>
    public static readonly AdornerOrder BehindForeground = new(280);

    /// <summary>
    ///     The adorner is in the foreground layer.
    /// </summary>
    public static readonly AdornerOrder Foreground = new(300);

    /// <summary>
    ///     The adorner is in the before foreground layer.
    /// </summary>
    public static readonly AdornerOrder BeforeForeground = new(400);

    private readonly int i;

    internal AdornerOrder(int i)
    {
        this.i = i;
    }

    /// <summary />
    public override int GetHashCode()
    {
        return i.GetHashCode();
    }

    /// <summary />
    public override bool Equals(object obj)
    {
        if (!(obj is AdornerOrder)) return false;
        return this == (AdornerOrder)obj;
    }

    /// <summary />
    public bool Equals(AdornerOrder other)
    {
        return i == other.i;
    }

    /// <summary>
    ///     Compares the <see cref="AdornerOrder" /> to another AdornerOrder.
    /// </summary>
    public int CompareTo(AdornerOrder other)
    {
        return i.CompareTo(other.i);
    }

    /// <summary />
    public static bool operator ==(AdornerOrder leftHandSide, AdornerOrder rightHandSide)
    {
        return leftHandSide.i == rightHandSide.i;
    }

    /// <summary />
    public static bool operator !=(AdornerOrder leftHandSide, AdornerOrder rightHandSide)
    {
        return leftHandSide.i != rightHandSide.i;
    }

    /// <summary />
    public static bool operator <(AdornerOrder leftHandSide, AdornerOrder rightHandSide)
    {
        return leftHandSide.i < rightHandSide.i;
    }

    /// <summary />
    public static bool operator <=(AdornerOrder leftHandSide, AdornerOrder rightHandSide)
    {
        return leftHandSide.i <= rightHandSide.i;
    }

    /// <summary />
    public static bool operator >(AdornerOrder leftHandSide, AdornerOrder rightHandSide)
    {
        return leftHandSide.i > rightHandSide.i;
    }

    /// <summary />
    public static bool operator >=(AdornerOrder leftHandSide, AdornerOrder rightHandSide)
    {
        return leftHandSide.i >= rightHandSide.i;
    }
}