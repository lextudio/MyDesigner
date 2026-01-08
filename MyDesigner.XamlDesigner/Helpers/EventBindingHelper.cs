using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MyDesigner.XamlDesigner.Helpers;

/// <summary>
/// Helper for binding Events to Code-Behind in Avalonia
/// </summary>
public class EventBindingHelper
{
    /// <summary>
    /// Create Event Handler in code-behind file
    /// </summary>
    public static (bool success, string filePath, int lineNumber) CreateEventHandler(
        string axamlFilePath,
        string controlName,
        string eventName,
        string handlerName = null)
    {
        try
        {
          
            if (string.IsNullOrEmpty(handlerName))
            {
                handlerName = string.IsNullOrEmpty(controlName)
                    ? $"{eventName}_Handler"
                    : $"{controlName}_{eventName}";
            }

          
            var csFilePath = axamlFilePath + ".cs";
            if (!File.Exists(csFilePath))
            {
               
                return (false, null, 0);
            }

           
            var content = File.ReadAllText(csFilePath);

            
            if (content.Contains($"void {handlerName}("))
            {
                 
                var lines = content.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains($"void {handlerName}("))
                    {
                        return (true, csFilePath, i + 1);
                    }
                }

                return (true, csFilePath, 1);
            }

          
            var classInfo = ExtractClassInfo(content);
            if (classInfo == null)
            {
               
                return (false, null, 0);
            }

           
            var eventHandler = GenerateEventHandler(handlerName, eventName);

           
            var insertPosition = FindInsertPosition(content, classInfo.Value.closingBracePosition);
            var newContent = content.Insert(insertPosition, eventHandler);

            
            File.WriteAllText(csFilePath, newContent);

          
            var lineNumber = content.Substring(0, insertPosition).Count(c => c == '\n') + 2;
              
            return (true, csFilePath, lineNumber);
        }
        catch (Exception ex)
        {
          
            return (false, null, 0);
        }
    }

    /// <summary>
    /// Extract class information from code
    /// </summary>
    private static (int closingBracePosition, string className)? ExtractClassInfo(string content)
    {
       
        var classMatch = Regex.Match(content, @"class\s+(\w+)\s*(?::\s*\w+)?");
        if (!classMatch.Success)
            return null;

        var className = classMatch.Groups[1].Value;
        var classStartIndex = classMatch.Index;

        
        var openBraceIndex = content.IndexOf('{', classStartIndex);
        if (openBraceIndex == -1)
            return null;

        
        int braceCount = 1;
        int closingBraceIndex = openBraceIndex + 1;

        while (closingBraceIndex < content.Length && braceCount > 0)
        {
            if (content[closingBraceIndex] == '{')
                braceCount++;
            else if (content[closingBraceIndex] == '}')
                braceCount--;

            if (braceCount == 0)
                break;

            closingBraceIndex++;
        }

        if (braceCount != 0)
            return null;

        return (closingBraceIndex, className);
    }

    /// <summary>
    /// Find appropriate insertion position
    /// </summary>
    private static int FindInsertPosition(string content, int closingBracePosition)
    {
       
        var beforeBrace = content.Substring(0, closingBracePosition);

   
        var lastMethodEnd = beforeBrace.LastIndexOf('}');

      
        if (lastMethodEnd != -1 && lastMethodEnd < closingBracePosition - 10)
        {
          
            var afterBrace = lastMethodEnd + 1;
            while (afterBrace < content.Length && (content[afterBrace] == '\r' || content[afterBrace] == '\n'))
            {
                afterBrace++;
            }
            return afterBrace;
        }

        
        var lineStart = closingBracePosition;
        while (lineStart > 0 && content[lineStart - 1] != '\n')
        {
            lineStart--;
        }
        return lineStart;
    }

    /// <summary>
    /// Generate Event Handler code for Avalonia
    /// </summary>
    private static string GenerateEventHandler(string handlerName, string eventName)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Event handler for {eventName}");
        sb.AppendLine("    /// </summary>");

        
        var signature = GetAvaloniaEventSignature(eventName);
        sb.AppendLine($"    private void {handlerName}({signature})");
        sb.AppendLine("    {");
        sb.AppendLine($"        // TODO: Implement {eventName} logic");
        sb.AppendLine("    }");

        return sb.ToString();
    }

    /// <summary>
    /// Get appropriate signature for Event in Avalonia
    /// </summary>
    private static string GetAvaloniaEventSignature(string eventName)
    {
       
        var avaloniaEvents = new Dictionary<string, string>
        {
            { "Click", "object sender, Avalonia.Interactivity.RoutedEventArgs e" },
            { "Loaded", "object sender, Avalonia.Interactivity.RoutedEventArgs e" },
            { "Unloaded", "object sender, Avalonia.Interactivity.RoutedEventArgs e" },
            { "PointerEntered", "object sender, Avalonia.Input.PointerEventArgs e" },
            { "PointerExited", "object sender, Avalonia.Input.PointerEventArgs e" },
            { "PointerPressed", "object sender, Avalonia.Input.PointerPressedEventArgs e" },
            { "PointerReleased", "object sender, Avalonia.Input.PointerReleasedEventArgs e" },
            { "PointerMoved", "object sender, Avalonia.Input.PointerEventArgs e" },
            { "KeyDown", "object sender, Avalonia.Input.KeyEventArgs e" },
            { "KeyUp", "object sender, Avalonia.Input.KeyEventArgs e" },
            { "TextChanged", "object sender, Avalonia.Controls.TextChangedEventArgs e" },
            { "SelectionChanged", "object sender, Avalonia.Controls.SelectionChangedEventArgs e" },
            { "Checked", "object sender, Avalonia.Interactivity.RoutedEventArgs e" },
            { "Unchecked", "object sender, Avalonia.Interactivity.RoutedEventArgs e" },
            { "ValueChanged", "object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e" },
            { "GotFocus", "object sender, Avalonia.Input.GotFocusEventArgs e" },
            { "LostFocus", "object sender, Avalonia.Interactivity.RoutedEventArgs e" },
            { "Tapped", "object sender, Avalonia.Input.TappedEventArgs e" },
            { "DoubleTapped", "object sender, Avalonia.Input.TappedEventArgs e" },
            { "PropertyChanged", "object sender, Avalonia.AvaloniaPropertyChangedEventArgs e" }
        };

        return avaloniaEvents.TryGetValue(eventName, out var signature)
            ? signature
            : "object sender, EventArgs e";
    }

    /// <summary>
    /// Update AXAML to add Event Handler
    /// </summary>
    public static bool UpdateAxamlWithEvent(string axamlContent, string controlName, string eventName, string handlerName)
    {
        try
        {
           
            var pattern = $@"<(\w+)[^>]*x:Name\s*=\s*""{controlName}""[^>]*>";
            var match = Regex.Match(axamlContent, pattern);

            if (!match.Success)
            {
            
                return false;
            }

          
            if (match.Value.Contains($"{eventName}="))
            {
              
                return true;
            }

          
            var updatedTag = match.Value.TrimEnd('>') + $" {eventName}=\"{handlerName}\">";
            var updatedAxaml = axamlContent.Replace(match.Value, updatedTag);

            return true;
        }
        catch (Exception ex)
        {
          
            return false;
        }
    }

    /// <summary>
    /// Get list of available Events for Control in Avalonia
    /// </summary>
    public static List<EventInfo> GetAvailableAvaloniaEvents(Type controlType)
    {
        var events = new List<EventInfo>();

        try
        {
            var allEvents = controlType.GetEvents(BindingFlags.Public | BindingFlags.Instance);

           
            var commonAvaloniaEventNames = new[]
            {
                "Click", "PointerPressed", "PointerReleased", "PointerEntered", "PointerExited", "PointerMoved",
                "KeyDown", "KeyUp", "Loaded", "Unloaded", "AttachedToVisualTree", "DetachedFromVisualTree",
                "TextChanged", "SelectionChanged", "Checked", "Unchecked",
                "ValueChanged", "GotFocus", "LostFocus", "Tapped", "DoubleTapped",
                "PropertyChanged", "DataContextChanged"
            };

            events = allEvents
                .Where(e => commonAvaloniaEventNames.Contains(e.Name))
                .OrderBy(e => e.Name)
                .ToList();
        }
        catch (Exception ex)
        {
           
        }

        return events;
    }

    /// <summary>
    /// Convert WPF Event to corresponding Avalonia Event
    /// </summary>
    public static string ConvertWpfEventToAvalonia(string wpfEventName)
    {
        var eventMapping = new Dictionary<string, string>
        {
            { "MouseEnter", "PointerEntered" },
            { "MouseLeave", "PointerExited" },
            { "MouseDown", "PointerPressed" },
            { "MouseUp", "PointerReleased" },
            { "MouseMove", "PointerMoved" },
            { "PreviewMouseDown", "PointerPressed" }, // Avalonia doesn't have Preview events
            { "PreviewMouseUp", "PointerReleased" },
            { "PreviewKeyDown", "KeyDown" },
            { "PreviewKeyUp", "KeyUp" }
        };

        return eventMapping.TryGetValue(wpfEventName, out var avaloniaEvent) ? avaloniaEvent : wpfEventName;
    }
}