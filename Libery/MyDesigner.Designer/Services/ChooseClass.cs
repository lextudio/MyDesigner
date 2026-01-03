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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace MyDesigner.Designer.Services;

public class ChooseClass : INotifyPropertyChanged
{
    private readonly ObservableCollection<Type> allClasses;
    private readonly ObservableCollection<Type> filteredClasses;

    private string filter;
    private bool showSystemClasses;

    public ChooseClass(IEnumerable<Assembly> assemblies)
    {
        var projectClasses = new List<Type>();
        
        foreach (var a in assemblies)
        foreach (var t in a.GetExportedTypes())
            if (t.IsClass)
            {
                if (t.IsAbstract) continue;
                if (t.IsNested) continue;
                if (t.IsGenericTypeDefinition) continue;
                if (t.GetConstructor(Type.EmptyTypes) == null) continue;
                projectClasses.Add(t);
            }

        projectClasses.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
        allClasses = new ObservableCollection<Type>(projectClasses);
        filteredClasses = new ObservableCollection<Type>(projectClasses);
    }

    public ObservableCollection<Type> Classes => filteredClasses;

    public string Filter
    {
        get => filter;
        set
        {
            filter = value;
            RefreshFilter();
            RaisePropertyChanged(nameof(Filter));
        }
    }

    public bool ShowSystemClasses
    {
        get => showSystemClasses;
        set
        {
            showSystemClasses = value;
            RefreshFilter();
            RaisePropertyChanged(nameof(ShowSystemClasses));
        }
    }

    private Type currentClass;
    public Type CurrentClass
    {
        get => currentClass;
        set
        {
            currentClass = value;
            RaisePropertyChanged(nameof(CurrentClass));
        }
    }

    private void RefreshFilter()
    {
        filteredClasses.Clear();
        
        foreach (var type in allClasses)
        {
            if (FilterPredicate(type))
            {
                filteredClasses.Add(type);
            }
        }
    }

    private bool FilterPredicate(Type c)
    {
        if (!ShowSystemClasses)
            if (c.Namespace.StartsWith("System") || c.Namespace.StartsWith("Microsoft"))
                return false;

        return Match(c.Name, Filter);
    }

    private static bool Match(string className, string filter)
    {
        if (string.IsNullOrEmpty(filter))
            return true;
        return className.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase);
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion
}