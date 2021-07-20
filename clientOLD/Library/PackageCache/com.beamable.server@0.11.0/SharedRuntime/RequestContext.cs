using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Api;

namespace Beamable.Server
{
   public class RequestContext : IUserContext
   {
      public string Cid { get; }
      public string Pid { get; }
      public long Id { get; }
      public int Status { get; }
      public long UserId { get; }
      public string Path { get; }
      public string Method { get; }
      public string Body { get; }
      public HashSet<string> Scopes { get; }

      public bool HasScopes(IEnumerable<string> scopes) => HasScopes(scopes.ToArray());
      public bool HasScopes(params string[] scopes)
      {
         if (Scopes.Contains("*")) return true;
         var missingCount = scopes.Count(required => !Scopes.Contains(required));
         return missingCount == 0;
      }

      public void CheckAdmin()
      {
         if (!HasScopes("*"))
            throw new MissingScopesException(Scopes);
      }

      public bool IsEvent => Path?.StartsWith("event/") ?? false;

      public RequestContext(string cid, string pid, long id, int status, long userId, string path, string method, string body, HashSet<string> scopes=null)
      {
         Cid = cid;
         Pid = pid;
         Id = id;
         UserId = userId;
         Path = path;
         Method = method;
         Status = status;
         Body = body;
         Scopes = scopes ?? new HashSet<string>();
         Scopes.RemoveWhere(string.IsNullOrEmpty);
      }

   }
}