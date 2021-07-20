using System;
using Beamable.Common;
using Beamable.Common.Api;

namespace Beamable.Server
{
   public delegate TMicroService ServiceFactory<out TMicroService>() where TMicroService : Microservice;
   public delegate IBeamableRequester RequesterFactory(RequestContext ctx);
   public delegate IBeamableServices ServicesFactory(IBeamableRequester requester, RequestContext ctx);

   public struct RequestHandlerData
   {
      public RequestContext Context;
      public IBeamableRequester Requester;
      public IBeamableServices Services;
   }

   public abstract class Microservice
   {
      protected RequestContext Context;
      protected IBeamableRequester Requester;
      protected IBeamableServices Services;
      private RequesterFactory _requesterFactory;
      private ServicesFactory _servicesFactory;

      public void ProvideContext(RequestContext ctx)
      {
         Context = ctx;
      }

      public void ProvideRequester(RequesterFactory requesterFactory)
      {
         _requesterFactory = requesterFactory;
         Requester = _requesterFactory(Context);
      }

      public void ProvideServices(ServicesFactory servicesFactory)
      {
         _servicesFactory = servicesFactory;
         Services = _servicesFactory(Requester, Context);
      }

      protected RequestHandlerData AssumeUser(long userId)
      {
         // require admin privs.
         Context.CheckAdmin();

         var newCtx = new RequestContext(
            Context.Cid, Context.Pid, Context.Id, Context.Status, userId, Context.Path, Context.Method, Context.Body,
            Context.Scopes);
         var requester = _requesterFactory(newCtx);
         var services = _servicesFactory(requester, newCtx);
         return new RequestHandlerData
         {
            Context = newCtx,
            Requester = requester,
            Services = services
         };
      }
   }
}
