using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDesigner.Designer.PropertyGrid;

namespace MyDesigner.XamlDesigner.Views.Tools
{
    public partial class BoundPropertyGridView : UserControl
    {
        public BoundPropertyGridView()
        {
            InitializeComponent();
            // Exact same as original: DataContext = Shell.Instance;
            DataContext = Shell.Instance;
            
            // Exact same as original: Shell.Instance.PropertyGrid = uxPropertyGridView.PropertyGrid;
            var propertyGridView = this.FindControl<PropertyGridView>("uxPropertyGridView");
            if (propertyGridView != null)
            {
                Shell.Instance.PropertyGrid = propertyGridView.PropertyGrid;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}