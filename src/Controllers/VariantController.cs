using System;
using System.Collections.Generic;
using System.Linq;
using TFE.CopyVariantContent.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace TFE.CopyVariantContent.Controllers
{
    [PluginController("Variants")]
    public class VariantController : UmbracoAuthorizedApiController
    {
        private readonly IContentService _contentService;
        private readonly ILocalizationService _localizationService;

        public VariantController(IContentService contentService, ILocalizationService localizationService)
        {
            _contentService = contentService;
            _localizationService = localizationService;
        }

        public VariantResult CreateContentVariants(int? nodeId, bool includeChilderen)
        {
            try
            {

                if (nodeId is null)
                {
                    return new VariantResult
                    {
                        IsSucces = false,
                        Error = new Error
                        {
                            Message = "NodeId is not set",
                        },
                    };
                }

                var contentItems = new List<IContent>();
                var contentItem = _contentService.GetById(nodeId.Value);

                if (contentItem == null)
                {
                    return new VariantResult
                    {
                        IsSucces = false,
                        Error = new Error
                        {
                            Message = "content is not found",
                        },
                    };
                }

                contentItems.Add(contentItem);

                if (includeChilderen)
                {
                    contentItems.AddRange(_contentService.GetPagedChildren(contentItem.Id, 0, 1000000, out long totalRecords));
                }

                // returns all languages currently active on this Umbraco instance
                var languages = _localizationService.GetAllLanguages();

                foreach (var content in contentItems)
                {
                    // returns all created languages for this content item
                    var contentItemLanguages = content.AvailableCultures;

                    foreach (var culture in languages.Select(x => x.CultureInfo.Name))
                    {
                        var isCreated = contentItemLanguages.Any(x => x == culture.ToLower());

                        // There is already a content item created for this language so skip
                        if (isCreated) continue;

                        content.SetCultureName(content.Name, culture);
                        foreach (var property in content.Properties)
                        {

                            var value = property.Values.FirstOrDefault()?.PublishedValue;
                            if (value is not null)
                            {
                                try
                                {
                                    content.SetValue(property.Alias, value, culture);
                                } catch { continue; }
                            }
                        }

                        _contentService.Save(content);
                    }
                }

                return new VariantResult
                {
                    IsSucces = true,
                };

            } catch (Exception e)
            {
                return new VariantResult
                {
                    IsSucces = false,
                    Error = new Error
                    {
                        Message = e.Message,
                    },
                };
            }
        }
    }
}
