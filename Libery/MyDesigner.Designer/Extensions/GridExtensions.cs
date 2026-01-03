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

using Avalonia.Controls;

namespace MyDesigner.Designer.Extensions;

/// <summary>
/// Extension methods for Grid definitions to provide Offset properties for Avalonia 11.x compatibility
/// </summary>
public static class GridExtensions
{
    /// <summary>
    /// Extension property to access Offset for ColumnDefinition
    /// </summary>
    public static double Offset(this ColumnDefinition columnDefinition)
    {
        // In Avalonia 11.x, we need to calculate the offset manually
        // This is a simplified implementation - may need refinement
        // We need to find the grid that contains this column definition
        // This is a workaround since Parent property doesn't exist
        
        // For now, return 0 as a fallback - this needs proper implementation
        // based on how the grid system works in the application
        return 0;
    }

    /// <summary>
    /// Extension method to access Offset for ColumnDefinition (method version)
    /// </summary>
    public static double GetOffset(this ColumnDefinition columnDefinition)
    {
        return columnDefinition.Offset();
    }

    /// <summary>
    /// Extension property to access Offset for RowDefinition
    /// </summary>
    public static double Offset(this RowDefinition rowDefinition)
    {
        // In Avalonia 11.x, we need to calculate the offset manually
        // This is a simplified implementation - may need refinement
        // We need to find the grid that contains this row definition
        // This is a workaround since Parent property doesn't exist
        
        // For now, return 0 as a fallback - this needs proper implementation
        // based on how the grid system works in the application
        return 0;
    }

    /// <summary>
    /// Extension method to access Offset for RowDefinition (method version)
    /// </summary>
    public static double GetOffset(this RowDefinition rowDefinition)
    {
        return rowDefinition.Offset();
    }
}