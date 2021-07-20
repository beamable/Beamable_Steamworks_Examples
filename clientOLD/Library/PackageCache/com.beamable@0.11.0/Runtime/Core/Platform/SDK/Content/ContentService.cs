using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Content;
using Beamable.Common.Content;

namespace Beamable.Content
{
   public class ManifestSubscription : PlatformSubscribable<ClientManifest, ClientManifest>
   {
      private ClientManifest _latestManifiest;

      private Promise<Unit> _manifestPromise = new Promise<Unit>();
      private Dictionary<string, ClientContentInfo> _contentIdTable = new Dictionary<string, ClientContentInfo>();
      private Dictionary<Type, ContentCache> _contentCaches = new Dictionary<Type, ContentCache>();

      public ManifestSubscription(PlatformService platform, IBeamableRequester requester) : base(platform, requester, "content")
      {

      }

      public bool TryGetContentId(string contentId, out ClientContentInfo clientInfo)
      {
         return _contentIdTable.TryGetValue(contentId, out clientInfo);
      }


      public Promise<ClientManifest> GetManifest()
      {
         return _manifestPromise.Map(x => _latestManifiest);
      }

      public Promise<ClientManifest> GetManifest(ContentQuery query)
      {
         return _manifestPromise.Map(x => _latestManifiest.Filter(query));
      }

      public Promise<ClientManifest> GetManifest(string filter)
      {
         return _manifestPromise.Map(x => _latestManifiest.Filter(filter));
      }

      protected override string CreateRefreshUrl(string scope)
      {
         return "/basic/content/manifest/public";
      }

      protected override Promise<ClientManifest> ExecuteRequest(IBeamableRequester requester, string url)
      {
         return requester.Request(Method.GET, url, null, true, ClientManifest.ParseCSV, useCache:true);
      }

      protected override void OnRefresh(ClientManifest data)
      {
         Notify(data);
         _latestManifiest = data;

         // TODO work on better refresh strategy. A total wipe isn't very performant.
         _contentIdTable.Clear();
         foreach (var entry in _latestManifiest.entries)
         {
            _contentIdTable.Add(entry.contentId, entry);
         }

         _manifestPromise.CompleteSuccess(new Unit());
      }

   }

   /// <summary>
   /// This class defines the main entry point for the %Content feature.
   /// 
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   /// 
   /// #### Related Links
   /// - See the <a target="_blank" href="https://docs.beamable.com/docs/content-feature">Content</a> feature documentation
   /// - See Beamable.API script reference
   /// 
   /// ![img beamable-logo]
   /// 
   /// </summary>
   public class ContentService : IContentApi, IHasPlatformSubscriber<ManifestSubscription, ClientManifest, ClientManifest> //PlatformSubscribable<ClientManifest, ClientManifest>
   {

      public IBeamableRequester Requester { get; }
      public ManifestSubscription Subscribable { get; }

      private Dictionary<Type, ContentCache> _contentCaches = new Dictionary<Type, ContentCache>();

      public ContentService(PlatformService platform, IBeamableRequester requester) //: base(platform, requester, "content")
      {
         Requester = requester;
         Subscribable = new ManifestSubscription(platform, requester);

         Subscribable.Subscribe(cb =>
         {
            // pay attention, server...
         });
      }

      public Promise<TContent> GetContent<TContent>(IContentRef<TContent> reference)
         where TContent : ContentObject, new()
      {
         return GetContent<TContent>(reference as IContentRef);
      }

      public Promise<IContentObject> GetContent(string contentId, Type contentType)
      {
         if (string.IsNullOrEmpty(contentId))
         {
            return Promise<IContentObject>.Failed(new ContentNotFoundException(contentId));
         }
#if UNITY_EDITOR
         if (!UnityEditor.EditorApplication.isPlaying)
         {
            BeamableLogger.LogError("Cannot resolve content at edit time!");
            throw new Exception("Cannot resolve content at edit time!");
         }
#endif
         ContentCache rawCache;

         if (!_contentCaches.TryGetValue(contentType, out rawCache))
         {
            var cacheType = typeof(ContentCache<>).MakeGenericType(contentType);
            var constructor = cacheType.GetConstructor(new[] { typeof(IBeamableRequester) });
            rawCache = (ContentCache)constructor.Invoke(new[] {Requester});

            _contentCaches.Add(contentType, rawCache);
         }


         return GetManifest().FlatMap(manifest =>
         {
            if (!Subscribable.TryGetContentId(contentId, out var info))
            {
               return Promise<IContentObject>.Failed(new ContentNotFoundException(contentId));
            }
            return rawCache.GetContentObject(info);
         });

      }

      public Promise<IContentObject> GetContent(string contentId)
      {
         var referencedType = ContentRegistry.GetTypeFromId(contentId);
         return GetContent(contentId, referencedType);
      }

      public Promise<IContentObject> GetContent(IContentRef reference)
      {
         var referencedType = ContentRegistry.GetTypeFromId(reference.GetId());
         return GetContent(reference.GetId(), referencedType);
      }

      public Promise<TContent> GetContent<TContent>(IContentRef reference)
         where TContent : ContentObject, new()
      {
         if (reference == null || string.IsNullOrEmpty(reference.GetId()))
         {
            return Promise<TContent>.Failed(new ContentNotFoundException());
         }
         var referencedType = reference.GetReferencedType();
         return GetContent(reference.GetId(), referencedType).Map( c => (TContent)c);
      }

      public Promise<ClientManifest> GetManifest() => Subscribable.GetManifest();

      public Promise<ClientManifest> GetManifest(ContentQuery query) => Subscribable.GetManifest(query);

      public Promise<ClientManifest> GetManifest(string filter) => Subscribable.GetManifest(filter);

      public Promise<ClientManifest> GetCurrent(string scope = "") => Subscribable.GetCurrent(scope);
   }
}