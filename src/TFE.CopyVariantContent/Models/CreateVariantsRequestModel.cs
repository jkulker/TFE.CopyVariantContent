namespace TFE.CopyVariantContent.Models;

/// <summary>
/// Request to create language variants for a content item.
/// </summary>
public class CreateVariantsRequestModel
{
    /// <summary>
    /// The unique key of the content item to copy from.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// When true, the operation also runs for every descendant of the content item.
    /// </summary>
    public bool IncludeChildren { get; set; }
}
