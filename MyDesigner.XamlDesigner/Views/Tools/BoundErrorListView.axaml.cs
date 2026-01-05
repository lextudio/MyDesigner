using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyDesigner.XamlDesigner.Views.Tools
{
    public partial class BoundErrorListView : UserControl
    {
        public BoundErrorListView()
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