using Avalonia;
using Avalonia.Controls.Primitives;
using MyDesigner.Designer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDesigner.XamlDesigner.Tools
{
    public class EnumButton : ToggleButton
    {
        protected override Type StyleKeyOverride => typeof(EnumButton);

        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<EnumButton, object>(nameof(Value));

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}