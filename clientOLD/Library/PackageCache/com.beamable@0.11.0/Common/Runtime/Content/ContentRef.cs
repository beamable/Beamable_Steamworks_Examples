using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Api.Content;
using UnityEngine;

namespace Beamable.Common.Content
{
   public interface IContentLink
   {
      string GetId();
      void SetId(string id);
      void OnCreated();
   }

   [System.Serializable]
   public abstract class AbsContentLink<TContent> : AbsContentRef<TContent>, IContentLink where TContent : IContentObject, new()
   {
      public abstract void OnCreated(); // the resolution of this method is different based on client/server...
   }

   [System.Serializable]
   public abstract class BaseContentRef : IContentRef
   {
      public abstract string GetId();

      public abstract void SetId(string id);

      public abstract bool IsContent(IContentObject content);

      public abstract Type GetReferencedType();

      public abstract Type GetReferencedBaseType();

   }

   [System.Serializable]
   public abstract class AbsContentRef<TContent> : BaseContentRef, IContentRef<TContent> where TContent : IContentObject, new()
   {
      public string Id;

      public abstract Promise<TContent> Resolve(); // the resolution of this method is different based on client/server.

      public override string GetId()
      {
         return Id;
      }

      public override void SetId(string id)
      {
         Id = id;
      }

      public override bool IsContent(IContentObject content)
      {
         return content.Id.Equals(Id);
      }

      public override Type GetReferencedType()
      {
         if (string.IsNullOrEmpty(Id))
            return typeof(TContent);
         return ContentRegistry.GetTypeFromId(Id);
      }

      public override Type GetReferencedBaseType()
      {
         return typeof(TContent);
      }
   }

   public interface IContentRef
   {
      string GetId();
      void SetId(string id);
      bool IsContent(IContentObject content);
      Type GetReferencedType();
      Type GetReferencedBaseType();
   }

   public interface IContentRef<TContent> : IContentRef where TContent : IContentObject, new()
   {
      Promise<TContent> Resolve();
   }

   public static class IContentRefExtensions
   {
      public static SequencePromise<IContentObject> ResolveAll(this IEnumerable<IContentRef> refs, int batchSize = 50)
      {
         var seqPromise = new SequencePromise<IContentObject>(refs.Count());

         var x = ContentApi.Instance.FlatMap<SequencePromise<IContentObject>, IList<IContentObject>>(api =>
         {
            var promiseGenerators =
               refs.Select(r => new Func<Promise<IContentObject>>(() => api.GetContent(r))).ToList();
            var seq = Promise.ExecuteRolling(batchSize, promiseGenerators);
            seq.OnElementSuccess(seqPromise.ReportEntrySuccess);
            seq.OnElementError(seqPromise.ReportEntryError);
            return seq;
         }, () => seqPromise);

         return x;
      }
   }

   public class ContentRef : BaseContentRef
   {
      private readonly Type _contentType;
      private string _id;

      public ContentRef(Type contentType, string id)
      {
         _contentType = contentType;
         _id = id;
      }


      public override string GetId() => _id;

      public override void SetId(string id) => _id = id;

      public override bool IsContent(IContentObject content)
      {
         return content.Id.Equals(_id);
      }

      public override Type GetReferencedType()
      {
         if (string.IsNullOrEmpty(_id))
            return _contentType;
         return ContentRegistry.GetTypeFromId(_id);
      }

      public override Type GetReferencedBaseType()
      {
         return _contentType;
      }
   }

   [System.Serializable]
   public class ContentRef<TContent> : AbsContentRef<TContent> where TContent : ContentObject, IContentObject, new()
   {
      private Promise<TContent> _promise;

      public override Promise<TContent> Resolve()
      {
         return _promise ?? (_promise = ContentApi.Instance.FlatMap(service => service.GetContent(this)));
      }
   }

   [System.Serializable]
   public class ContentLink<TContent> : AbsContentLink<TContent> where TContent : ContentObject, IContentObject, new()
   {
      private Promise<TContent> _promise;

      public override Promise<TContent> Resolve()
      {
         return _promise ?? (_promise = ContentApi.Instance.FlatMap(service => service.GetContent(this)));
      }

      public override void OnCreated()
      {
         if (Application.isPlaying)
         {
            Resolve();
         }
      }
   }
}