using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyDesigner.XamlDesigner.Tools
{
    public partial class BitmapButton : Button
    {
        public BitmapButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string ImageHover
        {
            get { return "Images/" + GetType().Name + ".Hover.png"; }
        }

        public string ImageNormal
        {
            get { return "Images/" + GetType().Name + ".Normal.png"; }
        }

        public string ImagePressed
        {
            get { return "Images/" + GetType().Name + ".Pressed.png"; }
        }

        public string ImageDisabled
        {
            get { return "Images/" + GetType().Name + ".Disabled.png"; }
        }
    }

    public class CloseButton : BitmapButton
    {
    }
}