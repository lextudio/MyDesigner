using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyDesigner.XamlDesigner.Views.Tools
{
    public partial class BoundOutlineView : UserControl
    {
        public BoundOutlineView()
        {
            InitializeComponent();
            // Exact same as original: DataContext = Shell.Instance;
            DataContext = Shell.Instance;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}