using Avalonia.Controls;
using Avalonia.Interactivity;
//using HMIControl.HMI.Model;
//using MyStudio.Monitor;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     Interaction logic for TagComplexContextMenu.axaml
/// </summary>
public partial class TagComplexContextMenu : ContextMenu
{
    private readonly DesignItem designItem;

    public TagComplexContextMenu(DesignItem designItem)
    {
        this.designItem = designItem;
        InitializeComponent();
    }

    private void Click_EditStyle(object sender, RoutedEventArgs e)
    {
        var cg = designItem.OpenGroup("Complex Editor");

        var element = designItem.View;

        //var tagReader = element as ITagReader;
        //var frm = new TagComplexEditor(tagReader, tagReader.TagReadText);
        //frm.ShowDialog();
        //var txt = frm.TagText;
        //if (!string.IsNullOrEmpty(txt)) designItem.Properties.GetProperty("TagReadText").SetValue(txt);
        //else if (txt == string.Empty) designItem.Properties.GetProperty("TagReadText").SetValue(string.Empty);
    }
}