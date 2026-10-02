using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace TFE.CopyVariantContent.Controllers;

/// <summary>
/// Base controller for the Copy Variant Content backoffice API. Routes under
/// /umbraco/copyvariantcontent/api/v{version} and requires content section access.
/// </summary>
[ApiController]
[BackOfficeRoute("copyvariantcontent/api/v{version:apiVersion}")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
[MapToApi(Constants.ApiName)]
public abstract class CopyVariantContentApiControllerBase : ControllerBase
{
}
