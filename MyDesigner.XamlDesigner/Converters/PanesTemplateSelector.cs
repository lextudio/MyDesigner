using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace MyDesigner.XamlDesigner.Converters
{
    public class PanesTemplateSelector : IDataTemplate
    {
        public IDataTemplate DocumentTemplate { get; set; }

        public Control Build(object data)
        {
            if (data is Document && DocumentTemplate != null)
                return DocumentTemplate.Build(data);

            return new TextBlock { Text = "No template found" };
        }

        public bool Match(object data)
        {
            return data is Document;
        }
    }
}