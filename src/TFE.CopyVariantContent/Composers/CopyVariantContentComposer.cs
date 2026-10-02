using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using TFE.CopyVariantContent.Services;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace TFE.CopyVariantContent.Composers;

/// <summary>
/// Registers the variant copier service and a dedicated Swagger document for the package API.
/// </summary>
public class CopyVariantContentComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IVariantContentCopier, VariantContentCopier>();

        builder.Services.AddSingleton<IOperationIdHandler, CopyVariantContentOperationIdHandler>();

        builder.Services.Configure<SwaggerGenOptions>(options =>
        {
            // A dedicated Swagger document keeps the package endpoints out of the core
            // Management API document and lets us generate a typed TypeScript client.
            options.SwaggerDoc(Constants.ApiName, new OpenApiInfo
            {
                Title = "Copy Variant Content API",
                Version = "1.0",
            });

            options.OperationFilter<CopyVariantContentSecurityFilter>();
        });
    }

    /// <summary>
    /// Applies Umbraco backoffice authentication to the package Swagger document.
    /// </summary>
    private sealed class CopyVariantContentSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase
    {
        protected override string ApiName => Constants.ApiName;
    }

    /// <summary>
    /// Produces concise operation IDs for the package controllers so the generated client has clean method names.
    /// </summary>
    private sealed class CopyVariantContentOperationIdHandler : OperationIdHandler
    {
        public CopyVariantContentOperationIdHandler(IOptions<ApiVersioningOptions> apiVersioningOptions)
            : base(apiVersioningOptions)
        {
        }

        protected override bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
            => controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith(
                "TFE.CopyVariantContent.Controllers",
                StringComparison.InvariantCultureIgnoreCase) is true;

        public override string Handle(ApiDescription apiDescription)
            => $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
    }
}
