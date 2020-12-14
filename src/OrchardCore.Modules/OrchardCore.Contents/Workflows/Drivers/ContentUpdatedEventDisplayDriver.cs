using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class ContentUpdatedEventDisplayDriver : ContentEventDisplayDriverDriver<ContentUpdatedEvent, ContentUpdatedEventViewModel>
    {
        public ContentUpdatedEventDisplayDriver(IContentDefinitionManager contentDefinitionManager) : base(contentDefinitionManager)
        {
        }
    }
}
