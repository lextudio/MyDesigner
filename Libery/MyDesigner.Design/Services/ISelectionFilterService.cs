namespace MyDesigner.Design.Services;

/// <summary>
///     Service for filtering selected elements.
/// </summary>
public interface ISelectionFilterService
{
    /// <summary>
    ///     Filters the selected elements.
    /// </summary>
    ICollection<DesignItem> FilterSelectedElements(ICollection<DesignItem> items);
}