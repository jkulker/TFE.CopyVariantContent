namespace TFE.CopyVariantContent.Services;

/// <summary>
/// Outcome of a variant copy operation.
/// </summary>
public sealed class VariantCopyResult
{
    private VariantCopyResult(bool success, int variantsCreated, string? error)
    {
        Success = success;
        VariantsCreated = variantsCreated;
        Error = error;
    }

    public bool Success { get; }

    public int VariantsCreated { get; }

    public string? Error { get; }

    public static VariantCopyResult Ok(int variantsCreated) => new(true, variantsCreated, null);

    public static VariantCopyResult NotFound() => new(false, 0, "Content not found");
}
