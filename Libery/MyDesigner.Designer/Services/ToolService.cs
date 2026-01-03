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

namespace MyDesigner.Designer.Services;

// See IToolService for description.
internal sealed class DefaultToolService : IToolService, IDisposable
{
    private ITool _currentTool;

    public DefaultToolService(DesignContext context)
    {
        _currentTool = PointerTool;
        context.Services.RunWhenAvailable(
            delegate(IDesignPanel designPanel)
            {
                DesignPanel = designPanel;
                _currentTool.Activate(designPanel);
            });
    }

    public IDesignPanel DesignPanel { get; private set; }

    public void Dispose()
    {
        if (DesignPanel != null)
        {
            _currentTool.Deactivate(DesignPanel);
            DesignPanel = null;
        }
    }

    public Func<Visual, bool> DesignPanelHitTestFilterCallback
    {
        set => DesignPanel.CustomHitTestFilterBehavior = value;
    }

    public ITool PointerTool => Services.PointerTool.Instance;

    public ITool CurrentTool
    {
        get => _currentTool;
        set
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (_currentTool == value) return;
            if (DesignPanel != null) _currentTool.Deactivate(DesignPanel);
            _currentTool = value;
            if (DesignPanel != null) _currentTool.Activate(DesignPanel);
            if (CurrentToolChanged != null) CurrentToolChanged(this, EventArgs.Empty);
        }
    }

    public event EventHandler CurrentToolChanged;
}