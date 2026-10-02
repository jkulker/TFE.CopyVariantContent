namespace TFE.CopyVariantContent.Services;

/// <summary>
/// Copies existing content into the language variants that do not yet exist.
/// </summary>
public interface IVariantContentCopier
{
    /// <summary>
    /// Creates the missing culture variants for the given content item.
    /// </summary>
    /// <param name="contentKey">The unique key of the source content item.</param>
    /// <param name="includeChildren">When true, descendants are processed as well.</param>
    Task<VariantCopyResult> CopyToMissingVariantsAsync(Guid contentKey, bool includeChildren);
}
