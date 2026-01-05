namespace MyDesigner.XamlDesigner.Tools
{
    public class XamlError
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = "Error";
    }
}