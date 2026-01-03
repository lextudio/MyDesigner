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

namespace MyDesigner.Design.Interfaces;

/// <summary>
///     Interface for outline nodes.
/// </summary>
public interface IOutlineNode
{
    /// <summary>
    ///     Gets the selection service.
    /// </summary>
    ISelectionService SelectionService { get; }
    
    /// <summary>
    ///     Gets or sets whether the node is expanded.
    /// </summary>
    bool IsExpanded { get; set; }
    
    /// <summary>
    ///     Gets or sets the design item.
    /// </summary>
    DesignItem DesignItem { get; set; }
    
    /// <summary>
    ///     Gets the services container.
    /// </summary>
    ServiceContainer Services { get; }
    
    /// <summary>
    ///     Gets or sets whether the node is selected.
    /// </summary>
    bool IsSelected { get; set; }
    
    /// <summary>
    ///     Gets or sets whether the node is visible at design time.
    /// </summary>
    bool IsDesignTimeVisible { get; set; }
    
    /// <summary>
    ///     Gets whether the node is locked at design time.
    /// </summary>
    bool IsDesignTimeLocked { get; }
    
    /// <summary>
    ///     Gets the name of the node.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    ///     Gets the child nodes.
    /// </summary>
    ObservableCollection<IOutlineNode> Children { get; }
    
    /// <summary>
    ///     Determines whether nodes can be inserted.
    /// </summary>
    bool CanInsert(IEnumerable<IOutlineNode> nodes, IOutlineNode after, bool copy);
    
    /// <summary>
    ///     Inserts nodes.
    /// </summary>
    void Insert(IEnumerable<IOutlineNode> nodes, IOutlineNode after, bool copy);
}