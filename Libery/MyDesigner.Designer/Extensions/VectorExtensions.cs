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

namespace MyDesigner.Designer.Extensions;

/// <summary>
/// Extension methods for Vector to handle missing methods in Avalonia 11.x
/// </summary>
public static class VectorExtensions
{
    /// <summary>
    /// Calculates the angle between two vectors in degrees
    /// </summary>
    public static double AngleBetween(Vector vector1, Vector vector2)
    {
        // Calculate the angle between two vectors using dot product and cross product
        var dot = vector1.X * vector2.X + vector1.Y * vector2.Y;
        var cross = vector1.X * vector2.Y - vector1.Y * vector2.X;
        
        var angle = Math.Atan2(cross, dot) * 180.0 / Math.PI;
        
        return angle;
    }
    
    /// <summary>
    /// Gets the length (magnitude) of a vector
    /// </summary>
    public static double Length(this Vector vector)
    {
        return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
    }
    
    /// <summary>
    /// Normalizes a vector to unit length
    /// </summary>
    public static Vector Normalize(this Vector vector)
    {
        var length = vector.Length();
        if (length == 0) return new Vector(0, 0);
        return new Vector(vector.X / length, vector.Y / length);
    }
}