namespace MyDesigner.XamlDom;

public class XamlElementLineInfo
{
    public XamlElementLineInfo(int lineNumber, int linePosition)
    {
        LineNumber = lineNumber;
        LinePosition = linePosition;
    }

    public int LineNumber { get; set; }
    public int LinePosition { get; set; }

    public int Position { get; set; }
    public int Length { get; set; }
}