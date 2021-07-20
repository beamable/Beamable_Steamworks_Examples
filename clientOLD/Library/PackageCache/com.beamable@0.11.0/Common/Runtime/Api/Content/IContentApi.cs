using System;
using Beamable.Common.Content;

namespace Beamable.Common.Api.Content
{
   public interface IContentApi : ISupportsGet<ClientManifest>
   {
      Promise<TContent> GetContent<TContent>(IContentRef<TContent> reference)
         where TContent : ContentObject, new();

      Promise<IContentObject> GetContent(string contentId, Type contentType);

      Promise<IContentObject> GetContent(string contentId);

      Promise<IContentObject> GetContent(IContentRef reference);

      Promise<TContent> GetContent<TContent>(IContentRef reference)
         where TContent : ContentObject, new();

      Promise<ClientManifest> GetManifest();

      Promise<ClientManifest> GetManifest(string filter);

      Promise<ClientManifest> GetManifest(ContentQuery query);
   }

   public static class ContentApi
   {
      // TODO: This is very hacky, but it lets use inject a different service in. Replace with ServiceManager (lot of unity deps to think about)
      public static Promise<IContentApi> Instance = new Promise<IContentApi>();
   }
}