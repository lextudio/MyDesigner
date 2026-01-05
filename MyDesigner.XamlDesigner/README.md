# MyDesigner XAML Designer - Avalonia Version

## Overview
This is the Avalonia version of the MyDesigner XAML Designer, converted from the original WPF version. The application provides a visual XAML designer with docking panels, property grid, toolbox, and document management.

## Key Features Converted

### ✅ Completed Features
1. **Main Window with Dock.Avalonia Layout**
   - Replaced WPF AvalonDock with Dock.Avalonia
   - Proper docking panels for Toolbox, Properties, Errors, etc.
   - Document tabs for multiple XAML files

2. **Document Management**
   - New, Open, Save, Save As functionality
   - Recent files list
   - Multiple document support with tabs

3. **Property Grid Integration**
   - Connected to MyDesigner.Designer library
   - Property editing for selected elements

4. **Toolbox**
   - Control palette for drag-and-drop design
   - Assembly loading support

5. **Error List**
   - XAML parsing error display
   - Error navigation support

6. **Settings System**
   - JSON-based settings storage
   - Window state persistence
   - Recent files management

7. **Command System**
   - All menu commands implemented
   - Keyboard shortcuts support
   - Command routing to design surface

8. **UI Controls Converted**
   - EnumBar → Avalonia version with proper styling
   - EnumButton → Custom toggle button for Avalonia
   - BitmapButton → Image-based button with state support

### 🔄 Partially Implemented
1. **Design Surface Commands**
   - Basic routing implemented
   - Cut/Copy/Paste need full implementation
   - Undo/Redo connected to UndoService

2. **Run Command**
   - Basic XAML extraction implemented
   - Preview window needs Avalonia-specific implementation

3. **Render to Bitmap**
   - File dialog implemented
   - Actual bitmap rendering needs Avalonia RenderTargetBitmap

### 📋 File Structure

```
MyDesigner.XamlDesigner/
├── Commands/
│   └── DesignSurfaceCommands.cs     # Command routing helpers
├── Configuration/
│   └── Settings.cs                  # JSON-based settings
├── Converters/
│   └── ActiveDocumentConverter.cs   # Data converters
├── EventHandlers/
│   └── MenuEventHandlers.cs         # UI event handlers
├── Images/                          # UI icons and images
├── TestFiles/                       # Sample XAML files
├── Themes/
│   └── Generic.axaml               # Custom control styles
├── App.axaml/.cs                   # Application entry point
├── MainWindow.axaml/.cs            # Main application window
├── DocumentView.axaml/.cs          # Document editor view
├── ToolboxView.axaml/.cs           # Control toolbox
├── ErrorListView.axaml/.cs         # Error display
├── Shell.cs                        # Application shell/coordinator
├── Document.cs                     # Document model
├── Toolbox.cs                      # Toolbox logic
├── EnumBar.axaml/.cs              # Enum selection control
├── EnumButton.cs                   # Toggle button for enums
├── BitmapButton.axaml/.cs         # Image-based button
└── Various utility classes
```

### 🔧 Key Dependencies
- **Avalonia 11.3.10** - UI framework
- **Dock.Avalonia 11.3.8** - Docking system
- **CSharpEditor 1.1.6** - Code editor (replaces AvalonEdit)
- **DialogHost.Avalonia 0.10.3** - Modal dialogs
- **CommunityToolkit.Mvvm 8.4.0** - MVVM helpers
- **MyDesigner.Designer** - Core designer library
- **MyDesigner.Design** - Design-time services
- **MyDesigner.XamlDom** - XAML DOM manipulation

### 🚀 Usage
1. Build and run the application
2. Use File → New to create a new XAML document
3. Switch between Design and XAML views using the mode toggle
4. Drag controls from the Toolbox to the design surface
5. Edit properties in the Properties panel
6. View errors in the Error List panel

### 🔍 Testing
Sample XAML files are provided in the `TestFiles/` directory:
- `Sample1.xaml` - Basic controls example
- `Sample2.xaml` - Grid layout example

### 📝 Notes
- The conversion maintains compatibility with the existing MyDesigner.Designer library
- Settings are now stored in JSON format instead of .NET configuration
- Dock.Avalonia provides similar functionality to WPF's AvalonDock
- CSharpEditor replaces AvalonEdit for XAML text editing
- All WPF-specific code has been converted to Avalonia equivalents

### 🐛 Known Issues
- Some advanced design surface operations may need additional implementation
- Bitmap rendering functionality needs Avalonia-specific implementation
- Preview window (Run command) needs custom Avalonia window implementation

### 🔮 Future Enhancements
- Complete design surface command implementation
- Add more sample templates
- Implement advanced XAML features
- Add plugin system for custom controls
- Improve error handling and user feedback