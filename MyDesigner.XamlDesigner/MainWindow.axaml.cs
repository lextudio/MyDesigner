using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using MyDesigner.Design;
using MyDesigner.Designer;
using MyDesigner.Designer.Services;
using MyDesigner.Designer.Xaml;
using MyDesigner.XamlDom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace MyDesigner.XamlDesigner
{
    public enum DocumentMode
    {
        Xaml, Design
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
        // ����� ������� ������ (Property)
        public List<ToolBoxItem> ToolBoxItems { get; set; }
        private void lstControls_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var item = lstControls.SelectedItem as ToolBoxItem;
            if (item != null)
            {
                var tool = new CreateComponentTool(item.Type);
                designSurface.DesignPanel.Context.Services.Tool.CurrentTool = tool;
            }
        }

        private void lstControls_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var item = lstControls.SelectedItem as ToolBoxItem;
            if (item != null)
            {
                var tool = new CreateComponentTool(item.Type);
                designSurface.DesignPanel.Context.Services.Tool.CurrentTool = tool;
            }
        }


        private static string xaml = @"<Grid 
xmlns=""https://github.com/avaloniaui""
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
mc:Ignorable=""d""
x:Name=""rootElement""  Background=""White""></Grid>";

        public MainWindow()
        {
            InitializeComponent();


            // ��� ������� ��������
            ToolBoxItems = new List<ToolBoxItem>
            {
                new ToolBoxItem { Type = typeof(Button) },
                new ToolBoxItem { Type = typeof(TextBlock) },
                new ToolBoxItem { Type = typeof(TextBox) },
                new ToolBoxItem { Type = typeof(Grid) },
                new ToolBoxItem { Type = typeof(Canvas) },
                new ToolBoxItem { Type = typeof(ComboBox) },
                new ToolBoxItem { Type = typeof(ListBox) },
                new ToolBoxItem { Type = typeof(Avalonia.Controls.Shapes.Path) },
                new ToolBoxItem { Type = typeof(Line) },
                new ToolBoxItem { Type = typeof(Rectangle) },
                new ToolBoxItem { Type = typeof(Border) },
                new ToolBoxItem { Type = typeof(CheckBox) }
            };

            // ��� ������� ���� ListBox ������� �� ��� XAML
            lstControls = this.FindControl<ListBox>("lstControls");
            if (lstControls != null)
            {
                lstControls.ItemsSource = ToolBoxItems;
            }




            BasicMetadata.Register();

            var loadSettings = new XamlLoadSettings();
            loadSettings.DesignerAssemblies.Add(this.GetType().Assembly);

            DragFileToDesignPanelHelper.Install(designSurface, CreateItemsOnDragCallback);
            using (var xmlReader = XmlReader.Create(new StringReader(File.ReadAllText("NewFileTemplate.xaml"))))
            {
                designSurface.LoadDesigner(xmlReader, loadSettings);
            }
            Mode = DocumentMode.Design;

            enumBar.Value= Mode;
        }

        // ������: ������� ���� ���� DesignItem ����� (Wrapper) ��� �� WPF Designer
        private DesignItem[] CreateItemsOnDragCallback(DesignContext context, DragEventArgs e)
        {
            // 1. �� ������� 11+ ������ GetFiles() ����� �� FileDrop
            var storageItems = e.Data.GetFiles();

            if (storageItems == null || !storageItems.Any())
                return null;

            // 2. ����� ���� ������ (Control)
            var textBlock = new TextBlock();

            // 3. ����� ������ �� ���� ������ ����� ��
            // ������: RegisterComponentForDesigner �� ����� ����� �� ������ ��������
            var item = context.Services.Component.RegisterComponentForDesigner(textBlock);

            // 4. ����� ������� �������� ���� ������� (AvaloniaProperty)
            // ������ SetValue ������ ��� ��� DesignItem ��� ��� ���� ��� �� ��� ��� Control
            item.Properties.GetProperty(Layoutable.WidthProperty).SetValue(300.0);
            item.Properties.GetProperty(Layoutable.HeightProperty).SetValue(30.0);

            // 5. ������� �������� �������� ���
            var fileList = storageItems
                .Select(f => f.Path.LocalPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .ToArray();

            string content = string.Join(Environment.NewLine, fileList);

            // ����� �� ��� TextBlock
            item.Properties.GetProperty(TextBlock.TextProperty).SetValue(content);

            return new[] { item };
        }

        private async void StartDrag(PointerPressedEventArgs e, ToolBoxItem item)
        {
            // ����� ������ �����
            var dragData = new DataObject();
            dragData.Set("ObjectControl", item.Type);

            // ��� ����� ����� (������ �� ������� ������� �������)
            var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
        }

        private void Export()
        {
            var sb = new StringBuilder();
            using (var xmlWriter = new XamlXmlWriter(sb))
            {
                designSurface.SaveDesigner(xmlWriter);
            }
            var xamlCode = sb.ToString();
        }

        DocumentMode mode;

        public DocumentMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                RaisePropertyChanged("Mode");
            }
        }
    }
}