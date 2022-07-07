using System;
using TFE.CopyVariantContent.Handler;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace TFE.CopyVariantContent.Composer
{
    public class TreeNotificationComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddNotificationHandler<MenuRenderingNotification, TreeNotificationHandler>();
        }
    }
}
