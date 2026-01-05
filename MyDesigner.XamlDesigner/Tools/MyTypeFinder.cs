using System;
using System.Reflection;
using MyDesigner.XamlDom;

namespace MyDesigner.XamlDesigner.Tools
{
    public class MyTypeFinder : XamlTypeFinder
    {
        private static MyTypeFinder _instance = new MyTypeFinder();
        public static MyTypeFinder Instance => _instance;

        private MyTypeFinder()
        {
            // Register default assemblies
            RegisterAssembly(typeof(Avalonia.Controls.Button).Assembly);
            RegisterAssembly(typeof(Avalonia.Controls.Primitives.TemplatedControl).Assembly);
        }

        public override Assembly LoadAssembly(string name)
        {
            try
            {
                return Assembly.LoadFrom(name);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading assembly {name}: {ex.Message}");
                return null;
            }
        }

        public override XamlTypeFinder Clone()
        {
            return _instance;
        }
    }
}