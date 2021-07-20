using Beamable.Common;
using Beamable.Common.Api.Content;
using Beamable.Common.Content;

namespace Beamable.Server.Api.Content
{
   public interface IMicroserviceContentApi : IContentApi
   {
      Promise<TContent> Resolve<TContent>(IContentRef<TContent> reference) where TContent : IContentObject, new();
   }
}