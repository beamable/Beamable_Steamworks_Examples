using System;
using System.Collections.Generic;

namespace Beamable.Server
{
   [System.AttributeUsage(System.AttributeTargets.Method)]
   public class ClientCallableAttribute : System.Attribute
   {
      private string pathName = "";
      public HashSet<string> RequiredScopes { get; }

      public ClientCallableAttribute() : this("", null)
      {

      }

      public ClientCallableAttribute(string pathnameOverride="", string[] requiredScopes=null)
      {
         pathName = pathnameOverride;
         RequiredScopes = requiredScopes == null
            ? new HashSet<string>()
            : new HashSet<string>(requiredScopes);
      }

      public string PathName
      {
         set { pathName = value; }
         get { return pathName; }
      }
   }

   [System.AttributeUsage(System.AttributeTargets.Method)]
   public class AdminOnlyCallableAttribute : ClientCallableAttribute
   {
      public AdminOnlyCallableAttribute(string pathnameOverride = "") : base(pathnameOverride,
         requiredScopes: new[] {"*"})
      {

      }
   }

   [System.AttributeUsage(System.AttributeTargets.Method)]
   public class CustomResponseSerializationAttribute : Attribute
   {
      public virtual string SerializeResponse(object raw)
      {
         return raw.ToString();
      }
   }
}
