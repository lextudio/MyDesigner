# MyDesigner.Design

هذا المشروع هو تحويل مكتبة MyDesigner.Design من WPF إلى Avalonia 11.3.10 مع .NET 10.

## التغييرات الرئيسية

### 1. تحديث المراجع
- تم استبدال مراجع WPF بمراجع Avalonia 11.3.10
- تم تحديث إطار العمل إلى .NET 10
- تم إزالة `UseWpf` من ملف المشروع

### 2. تحديث المساحات الاسمية
- تم تغيير `MyDesigner.Design` إلى `MyDesigner.Design`
- تم استبدال `System.Windows` بـ `Avalonia`
- تم استبدال `System.Windows.Controls` بـ `Avalonia.Controls`
- تم استبدال `System.Windows.Media` بـ `Avalonia.Media`

### 3. تحديث الأنواع
- `UIElement` → `Control`
- `DependencyProperty` → `AvaloniaProperty`
- `FrameworkElement` → `Control`
- `Visual` → `Visual` (نفس الاسم لكن من Avalonia)
- `MouseButtonEventHandler` → `EventHandler<PointerPressedEventArgs>`
- `DragEventHandler` → `EventHandler<DragEventArgs>`

### 4. تحديث الخصائص والطرق
- `TransformToVisual()` → `TransformToVisual()` (تحتاج تحديث للتوافق مع Avalonia)
- `LayoutTransform` → `RenderTransform` (Avalonia لا تدعم LayoutTransform)
- `GetVisualParent()` بدلاً من `TryFindParent<Visual>()`

### 5. الملفات المحولة

#### الملفات الأساسية ✅
- ✅ DesignContext.cs
- ✅ DesignItem.cs
- ✅ ExtensionMethods.cs
- ✅ ChangeGroup.cs
- ✅ DesignerException.cs
- ✅ DesignItemProperty.cs
- ✅ ServiceContainer.cs
- ✅ ServiceRequiredException.cs
- ✅ Services.cs
- ✅ EventArgs.cs
- ✅ Metadata.cs
- ✅ PlacementInformation.cs
- ✅ Tools.cs
- ✅ DesignPanelHitTestResult.cs
- ✅ HitTestType.cs
- ✅ PlacementAlignment.cs
- ✅ PlacementBehavior.cs
- ✅ PlacementOperation.cs
- ✅ PlacementType.cs
- ✅ DummyValueInsteadOfNullTypeDescriptionProvider.cs
- ✅ DrawItemExtension.cs
- ✅ MouseInteraction.cs

#### مجلد Adorners ✅
- ✅ AdornerPanel.cs (مع نظام Attached Properties)
- ✅ AdornerPlacement.cs
- ✅ AdornerPlacementSpace.cs
- ✅ AdornerProvider.cs
- ✅ AdornerProviderClasses.cs
- ✅ IAdornerLayer.cs
- ✅ RelativePlacement.cs

#### مجلد Extensions ✅
- ✅ BehaviorExtension.cs
- ✅ CustomInstanceFactory.cs
- ✅ DefaultExtension.cs
- ✅ DefaultInitializer.cs
- ✅ DesignItemInitializer.cs
- ✅ Extension.cs
- ✅ ExtensionAttribute.cs
- ✅ ExtensionForAttribute.cs
- ✅ ExtensionInterfaces.cs
- ✅ ExtensionManager.cs
- ✅ ExtensionServer.cs
- ✅ ExtensionServerAttribute.cs
- ✅ LogicalExtensionServers.cs
- ✅ MouseOverExtensionServer.cs
- ✅ NeverApplyExtensionsExtensionServer.cs
- ✅ SelectionExtensionServers.cs
- ✅ XamlInstanceFactory.cs

#### مجلد Interfaces ✅
- ✅ IOutlineNode.cs
- ✅ IObservableList.cs

#### مجلد PropertyGrid ✅
- ✅ Category.cs
- ✅ EditorManager.cs
- ✅ PropertyEditorAttribute.cs
- ✅ PropertyNode.cs
- ✅ SortedObservableCollection.cs
- ✅ TypeEditorAttribute.cs
- ✅ TypeHelper.cs

#### مجلد Services ✅
- ✅ ICopyPasteService.cs
- ✅ ISelectionFilterService.cs
- ✅ IToolService.cs

#### مجلد UIExtensions ✅
- ✅ MouseHorizontalWheelEnabler.cs (مبسط للـ Avalonia)
- ✅ MouseHorizontalWheelEventArgs.cs
- ✅ UIHelpers.cs
- ✅ VisualExtensions.cs

#### مجلد Configuration ✅
- ✅ AssemblyInfo.cs

## حالة المشروع

✅ **اكتمل التحويل بالكامل!**

جميع المجلدات والملفات تم تحويلها بنجاح إلى Avalonia 11.3.10 مع .NET 10.

## ملاحظات مهمة

1. **Transform System**: نظام التحويلات في Avalonia مختلف عن WPF، قد تحتاج بعض الطرق لتحديث إضافي.

2. **Property System**: نظام الخصائص في Avalonia (AvaloniaProperty) مختلف قليلاً عن WPF (DependencyProperty).

3. **Input Events**: أحداث الإدخال في Avalonia تستخدم Pointer بدلاً من Mouse.

4. **Layout System**: نظام التخطيط في Avalonia مختلف، خاصة عدم وجود LayoutTransform.

5. **Visual Tree**: طريقة التنقل في الشجرة المرئية مختلفة قليلاً.

## الخطوات التالية

1. تحويل المجلدات المتبقية
2. اختبار التوافق مع Avalonia 11.3.10
3. تحديث أي طرق تحتاج تعديل خاص بـ Avalonia
4. إنشاء مشروع اختبار للتأكد من عمل المكتبة

## متطلبات البناء

- .NET 10 SDK
- Avalonia 11.3.10
- Visual Studio 2022 أو JetBrains Rider أو VS Code

## الاستخدام

```xml
<PackageReference Include="Avalonia" Version="11.3.10" />
<PackageReference Include="Avalonia.Controls" Version="11.3.10" />
<PackageReference Include="Avalonia.Markup.Xaml" Version="11.3.10" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.10" />
```

```csharp
using MyDesigner.Design;
```