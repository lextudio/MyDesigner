// Copyright (c) 2019 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Avalonia.Markup.Xaml;
using System.Xml;

namespace MyDesigner.XamlDom;

/// <summary>
///     Tokenizer for markup extension attributes.
///     [MS-XAML 6.6.7.1]
/// </summary>
internal sealed class MarkupExtensionTokenizer
{
    private readonly List<MarkupExtensionToken> tokens = new();
    private int pos;

    private string text;

    private MarkupExtensionTokenizer()
    {
    }

    public static List<MarkupExtensionToken> Tokenize(string text)
    {
        var t = new MarkupExtensionTokenizer();
        t.text = text;
        t.Parse();
        return t.tokens;
    }

    private void AddToken(MarkupExtensionTokenKind kind, string val)
    {
        tokens.Add(new MarkupExtensionToken(kind, val));
    }

    private void Parse()
    {
        AddToken(MarkupExtensionTokenKind.OpenBrace, "{");
        Expect('{');
        ConsumeWhitespace();
        CheckNotEOF();

        var b = new StringBuilder();
        while (pos < text.Length && !char.IsWhiteSpace(text, pos) && text[pos] != '}')
            b.Append(text[pos++]);
        AddToken(MarkupExtensionTokenKind.TypeName, b.ToString());

        ConsumeWhitespace();
        while (pos < text.Length)
        {
            switch (text[pos])
            {
                case '}':
                    AddToken(MarkupExtensionTokenKind.CloseBrace, "}");
                    pos++;
                    break;
                case '=':
                    AddToken(MarkupExtensionTokenKind.Equals, "=");
                    pos++;
                    break;
                case ',':
                    AddToken(MarkupExtensionTokenKind.Comma, ",");
                    pos++;
                    break;
                default:
                    MembernameOrString();
                    break;
            }

            ConsumeWhitespace();
        }
    }

    private void MembernameOrString()
    {
        var b = new StringBuilder();
        if (text[pos] == '"' || text[pos] == '\'')
        {
            var quote = text[pos++];
            CheckNotEOF();
            var lastBackslashPos = -1;
            while (!(text[pos] == quote && text[pos - 1] != '\\'))
            {
                var current = pos;
                var c = text[pos++];
                //check if string is \\ and that the last backslash is not the previously saved char, ie that \\\\ does not become \\\ but just \\
                var isEscapedBackslash =
                    string.Concat(text[current - 1], c) == "\\\\" && current - 1 != lastBackslashPos;
                if (c != '\\' || isEscapedBackslash)
                {
                    b.Append(c);
                    if (isEscapedBackslash)
                        lastBackslashPos = current;
                }

                CheckNotEOF();
            }

            pos++; // consume closing quote
            ConsumeWhitespace();
        }
        else
        {
            var braceTotal = 0;
            while (true)
            {
                CheckNotEOF();
                switch (text[pos])
                {
                    case '\\':
                        pos++;
                        CheckNotEOF();
                        b.Append(text[pos++]);
                        break;
                    case '{':
                        b.Append(text[pos++]);
                        braceTotal++;
                        break;
                    case '}':
                        if (braceTotal == 0) goto stop;
                        b.Append(text[pos++]);
                        braceTotal--;
                        break;
                    case ',':
                    case '=':
                        if (braceTotal == 0) goto stop;
                        b.Append(text[pos++]);
                        break;
                    default:
                        b.Append(text[pos++]);
                        break;
                }
            }

            stop: ;
        }

        CheckNotEOF();
        var valueText = b.ToString();
        if (text[pos] == '=')
            AddToken(MarkupExtensionTokenKind.Membername, valueText.Trim());
        else
            AddToken(MarkupExtensionTokenKind.String, valueText);
    }

    private void Expect(char c)
    {
        CheckNotEOF();
        if (text[pos] != c)
            throw new XamlMarkupExtensionParseException("Expected '" + c + "'");
        pos++;
    }

    private void ConsumeWhitespace()
    {
        while (pos < text.Length && char.IsWhiteSpace(text, pos))
            pos++;
    }

    private void CheckNotEOF()
    {
        if (pos >= text.Length)
            throw new XamlMarkupExtensionParseException("Unexpected end of markup extension");
    }
}

/// <summary>
///     Exception thrown when XAML loading fails because there is a syntax error in a markup extension.
/// </summary>
[Serializable]
public class XamlMarkupExtensionParseException : XamlLoadException
{
    /// <summary>
    ///     Create a new XamlMarkupExtensionParseException instance.
    /// </summary>
    public XamlMarkupExtensionParseException()
    {
    }

    /// <summary>
    ///     Create a new XamlMarkupExtensionParseException instance.
    /// </summary>
    public XamlMarkupExtensionParseException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Create a new XamlMarkupExtensionParseException instance.
    /// </summary>
    public XamlMarkupExtensionParseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    ///     Create a new XamlMarkupExtensionParseException instance.
    /// </summary>
    protected XamlMarkupExtensionParseException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

internal enum MarkupExtensionTokenKind
{
    OpenBrace,
    CloseBrace,
    Equals,
    Comma,
    TypeName,
    Membername,
    String
}

internal sealed class MarkupExtensionToken
{
    public readonly MarkupExtensionTokenKind Kind;
    public readonly string Value;

    public MarkupExtensionToken(MarkupExtensionTokenKind kind, string value)
    {
        Kind = kind;
        Value = value;
    }

    public override string ToString()
    {
        return "[" + Kind + " " + Value + "]";
    }
}

/// <summary>
///     [MS-XAML 6.6.7.2]
/// </summary>
internal static class MarkupExtensionParser
{
    public static XamlObject Parse(string text, XamlObject parent, XmlAttribute attribute)
    {
        var tokens = MarkupExtensionTokenizer.Tokenize(text);
        if (tokens.Count < 3
            || tokens[0].Kind != MarkupExtensionTokenKind.OpenBrace
            || tokens[1].Kind != MarkupExtensionTokenKind.TypeName
            || tokens[tokens.Count - 1].Kind != MarkupExtensionTokenKind.CloseBrace)
            throw new XamlMarkupExtensionParseException("Invalid markup extension");

        var typeResolver = parent.ServiceProvider.Resolver;

        var typeName = tokens[1].Value;
        var extensionType = typeResolver.Resolve(typeName + "Extension");
        if (extensionType == null) extensionType = typeResolver.Resolve(typeName);
        if (extensionType == null || !typeof(IMarkupExtension).IsAssignableFrom(extensionType))
            throw new XamlMarkupExtensionParseException("Unknown markup extension " + typeName + "Extension");

        var positionalArgs = new List<string>();
        var namedArgs = new List<KeyValuePair<string, string>>();
        for (var i = 2; i < tokens.Count - 1; i++)
            if (tokens[i].Kind == MarkupExtensionTokenKind.String)
            {
                positionalArgs.Add(tokens[i].Value);
            }
            else if (tokens[i].Kind == MarkupExtensionTokenKind.Membername)
            {
                if (tokens[i + 1].Kind != MarkupExtensionTokenKind.Equals
                    || tokens[i + 2].Kind != MarkupExtensionTokenKind.String)
                    throw new XamlMarkupExtensionParseException("Invalid markup extension");
                namedArgs.Add(new KeyValuePair<string, string>(tokens[i].Value, tokens[i + 2].Value));
                i += 2;
            }

        // Find the constructor with positionalArgs.Count arguments
        var ctors = extensionType.GetConstructors().Where(c => c.GetParameters().Length == positionalArgs.Count)
            .ToList();
        if (ctors.Count < 1)
            throw new XamlMarkupExtensionParseException("No constructor for " +
                                                        extensionType.FullName + " found that takes " +
                                                        positionalArgs.Count + " arguments");
        if (ctors.Count > 1)
            Debug.WriteLine("Multiple constructors for " +
                            extensionType.FullName + " found that take " + positionalArgs.Count + " arguments");

        var ctor = ctors[0];
        var defaultCtor = extensionType.GetConstructor(Type.EmptyTypes);
        var mappingToProperties = defaultCtor != null;
        var map = new List<PropertyInfo>();

        if (mappingToProperties)
            foreach (var param in ctor.GetParameters())
            {
                var prop = extensionType.GetProperty(param.Name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null)
                {
                    mappingToProperties = false;
                    break;
                }

                map.Add(prop);
            }

        object instance;
        if (mappingToProperties)
        {
            instance = defaultCtor.Invoke(null);
        }
        else
        {
            var ctorParamsInfo = ctor.GetParameters();
            var ctorParams = new object[ctorParamsInfo.Length];
            for (var i = 0; i < ctorParams.Length; i++)
            {
                var paramType = ctorParamsInfo[i].ParameterType;
                ctorParams[i] = XamlParser.CreateObjectFromAttributeText(positionalArgs[i], paramType, parent);
            }

            instance = ctor.Invoke(ctorParams);
            //TODO
            //XamlObject.ConstructorArgsProperty - args collection
            //Reinvoke ctor when needed
        }

        var result = parent.OwnerDocument.CreateObject(instance);
        if (attribute != null) result.XmlAttribute = attribute;
        result.ParentObject = parent;

        if (mappingToProperties)
            for (var i = 0; i < positionalArgs.Count; i++)
            {
                var a = parent.OwnerDocument.XmlDocument.CreateAttribute(map[i].Name);
                a.Value = positionalArgs[i];
                XamlParser.ParseObjectAttribute(result, a, false);
            }

        foreach (var pair in namedArgs)
        {
            var a = parent.OwnerDocument.XmlDocument.CreateAttribute(pair.Key);
            a.Value = pair.Value;
            XamlParser.ParseObjectAttribute(result, a, false);
        }

        return result;
    }
}