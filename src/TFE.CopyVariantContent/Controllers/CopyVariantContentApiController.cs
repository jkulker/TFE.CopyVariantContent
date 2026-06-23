using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TFE.CopyVariantContent.Models;
using TFE.CopyVariantContent.Services;

namespace TFE.CopyVariantContent.Controllers;

/// <summary>
/// Creates the missing language variants for a content item.
/// </summary>
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "TFE.CopyVariantContent")]
public class CopyVariantContentApiController : CopyVariantContentApiControllerBase
{
    private readonly IVariantContentCopier _variantContentCopier;

    public CopyVariantContentApiController(IVariantContentCopier variantContentCopier)
        => _variantContentCopier = variantContentCopier;

    /// <summary>
    /// Copies the default-culture content into every variant that does not yet exist.
    /// </summary>
    [HttpPost("create-variants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType<CreateVariantsResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVariants(CreateVariantsRequestModel model)
    {
        VariantCopyResult result =
            await _variantContentCopier.CopyToMissingVariantsAsync(model.Id, model.IncludeChildren);

        if (!result.Success)
        {
            return NotFound(result.Error);
        }

        return Ok(new CreateVariantsResponseModel { VariantsCreated = result.VariantsCreated });
    }
}
