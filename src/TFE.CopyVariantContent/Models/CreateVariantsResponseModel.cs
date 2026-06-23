namespace TFE.CopyVariantContent.Models;

/// <summary>
/// Result of a create-variants operation.
/// </summary>
public class CreateVariantsResponseModel
{
    /// <summary>
    /// The number of culture variants created across the processed content items.
    /// </summary>
    public int VariantsCreated { get; set; }
}
