using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace TFE.CopyVariantContent.Services;

/// <summary>
/// Copies the default-culture values of a content item into every language variant
/// that has not been created yet. The default culture is never overwritten.
/// </summary>
public sealed class VariantContentCopier : IVariantContentCopier
{
    // Descendants are read in pages so very large trees do not load in a single query.
    private const int DescendantPageSize = 500;

    private readonly IContentService _contentService;
    private readonly ILanguageService _languageService;
    private readonly ILogger<VariantContentCopier> _logger;

    public VariantContentCopier(
        IContentService contentService,
        ILanguageService languageService,
        ILogger<VariantContentCopier> logger)
    {
        _contentService = contentService;
        _languageService = languageService;
        _logger = logger;
    }

    public async Task<VariantCopyResult> CopyToMissingVariantsAsync(Guid contentKey, bool includeChildren)
    {
        IContent? root = _contentService.GetById(contentKey);
        if (root is null)
        {
            return VariantCopyResult.NotFound();
        }

        var defaultIsoCode = await _languageService.GetDefaultIsoCodeAsync();
        IEnumerable<ILanguage> languages = await _languageService.GetAllAsync();
        var isoCodes = languages.Select(language => language.IsoCode).ToArray();

        var created = 0;
        foreach (IContent content in GetContentItems(root, includeChildren))
        {
            created += CreateMissingVariants(content, isoCodes, defaultIsoCode);
        }

        return VariantCopyResult.Ok(created);
    }

    private IEnumerable<IContent> GetContentItems(IContent root, bool includeChildren)
    {
        yield return root;

        if (!includeChildren)
        {
            yield break;
        }

        // "Include all items below" means every descendant, not only direct children.
        foreach (IContent descendant in GetDescendants(root.Id))
        {
            yield return descendant;
        }
    }

    private IEnumerable<IContent> GetDescendants(int rootId)
    {
        long page = 0;
        long total;
        do
        {
            IEnumerable<IContent> batch =
                _contentService.GetPagedDescendants(rootId, page, DescendantPageSize, out total);

            foreach (IContent item in batch)
            {
                yield return item;
            }

            page++;
        }
        while (page * DescendantPageSize < total);
    }

    private int CreateMissingVariants(IContent content, IReadOnlyCollection<string> isoCodes, string defaultIsoCode)
    {
        // Only culture-varying content can hold language variants.
        if (!content.ContentType.VariesByCulture())
        {
            return 0;
        }

        var missingCultures = isoCodes
            .Where(isoCode => !ShouldSkip(content, isoCode, defaultIsoCode))
            .ToList();

        if (missingCultures.Count == 0)
        {
            return 0;
        }

        foreach (var isoCode in missingCultures)
        {
            CopyVariant(content, isoCode, defaultIsoCode);
        }

        // Persist all new cultures for this node in a single save.
        var result = _contentService.Save(content);
        if (result.Success)
        {
            return missingCultures.Count;
        }

        _logger.LogWarning("Could not save variants for content {Key}: {Status}", content.Key, result.Result);
        return 0;
    }

    private static bool ShouldSkip(IContent content, string isoCode, string defaultIsoCode)
    {
        // Never overwrite the source culture, and never touch a variant that already exists.
        return isoCode.Equals(defaultIsoCode, StringComparison.OrdinalIgnoreCase)
               || content.AvailableCultures.Contains(isoCode, StringComparer.OrdinalIgnoreCase);
    }

    private void CopyVariant(IContent content, string isoCode, string defaultIsoCode)
    {
        // A culture name is required; fall back through the invariant name to a safe default.
        var name = content.GetCultureName(defaultIsoCode) ?? content.Name ?? "Untitled";
        content.SetCultureName(name, isoCode);

        foreach (IProperty property in content.Properties)
        {
            CopyPropertyValue(content, property, isoCode, defaultIsoCode);
        }
    }

    private void CopyPropertyValue(IContent content, IProperty property, string isoCode, string defaultIsoCode)
    {
        // Invariant properties are shared across cultures, so there is nothing to copy.
        if (!property.PropertyType.VariesByCulture())
        {
            return;
        }

        var value = property.GetValue(defaultIsoCode);
        if (value is null)
        {
            return;
        }

        try
        {
            content.SetValue(property.Alias, value, isoCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not copy property {Alias} to culture {Culture}",
                property.Alias,
                isoCode);
        }
    }
}
