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

using System.Diagnostics.CodeAnalysis;

namespace MyDesigner.Designer;

/// <summary>
///     When the designer is hosted in an Avalonia application, exceptions in
///     drag'n'drop handlers are silently ignored.
///     Applications hosting the designer should listen to the event and provide their own exception handling
///     method. If no event listener is registered, exceptions will call Environment.FailFast.
/// </summary>
public static class DragDropExceptionHandler
{
    /// <summary>
    ///     Event that occurs when an unhandled exception occurs during drag'n'drop operations.
    /// </summary>
    public static event EventHandler<Exception> UnhandledException;

    /// <summary>
    ///     Raises the UnhandledException event, or calls Environment.FailFast if no event handlers are present.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "We're raising an event")]
    public static void RaiseUnhandledException(Exception exception)
    {
        if (exception == null)
            throw new ArgumentNullException("exception");
        
        var eh = UnhandledException;
        if (eh != null)
            eh(null, exception);
        else
            Environment.FailFast(exception.ToString());
    }
}