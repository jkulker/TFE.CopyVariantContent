using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace TFE.CopyVariantContent.Handler
{
    public class TreeNotificationHandler : INotificationHandler<MenuRenderingNotification>
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public TreeNotificationHandler(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        }

        public void Handle(MenuRenderingNotification notification)
        {
            if (notification.TreeAlias.Equals("content"))
            {
               // Creates a menu action that will open /umbraco/currentSection/itemAlias.html
               var menuItem = new Umbraco.Cms.Core.Models.Trees.MenuItem("createVariants", "Create variants");

               // optional, if you don't want to follow the naming conventions, but do want to use a angular view
               // you can also use a direct path "../App_Plugins/my/long/url/to/view.html"
               menuItem.AdditionalData.Add("actionView", "../App_Plugins/TFE.CopyVariantContent/variants_add.html");

               // sets the icon to icon-chat-active
               menuItem.Icon = "chat-active";

                // insert as last item
                var index = notification.Menu.Items.Count;
                notification.Menu.Items.Insert(index, menuItem);
            }
        }
    }
}
