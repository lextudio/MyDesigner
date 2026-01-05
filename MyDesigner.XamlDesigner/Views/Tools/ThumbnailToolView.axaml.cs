using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDesigner.XamlDesigner.ViewModels.Tools;

namespace MyDesigner.XamlDesigner.Views.Tools
{
    public partial class ThumbnailToolView : UserControl
    {
        public ThumbnailToolView()
        {
            InitializeComponent();
            DataContext = new ThumbnailToolViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}