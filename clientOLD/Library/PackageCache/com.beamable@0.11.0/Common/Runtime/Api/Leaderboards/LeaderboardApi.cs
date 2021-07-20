using System;
using System.Collections.Generic;
using System.Text;
using Beamable.Common.Leaderboards;
using Beamable.Serialization.SmallerJSON;

namespace Beamable.Common.Api.Leaderboards
{
   public class LeaderboardApi : ILeaderboardApi
   {
      private readonly UserDataCache<RankEntry>.FactoryFunction _factoryFunction;
      public IBeamableRequester Requester { get; }
      public IUserContext UserContext { get; }

      private static long TTL_MS = 60 * 1000;
      private Dictionary<string, UserDataCache<RankEntry>> caches = new Dictionary<string, UserDataCache<RankEntry>>();

      public LeaderboardApi(IBeamableRequester requester, IUserContext userContext, UserDataCache<RankEntry>.FactoryFunction factoryFunction)
      {
         _factoryFunction = factoryFunction;
         Requester = requester;
         UserContext = userContext;
      }

      public UserDataCache<RankEntry> GetCache(string boardId)
      {
         UserDataCache<RankEntry> cache;
         if (!caches.TryGetValue(boardId, out cache))
         {
            cache = _factoryFunction(
               $"Leaderboard.{boardId}",
               TTL_MS,
               (gamerTags => Resolve(boardId, gamerTags))
            );
            caches.Add(boardId, cache);
         }

         return cache;
      }


      public Promise<RankEntry> GetUser(LeaderboardRef leaderBoard, long gamerTag)
         => GetUser(leaderBoard.Id, gamerTag);
      public Promise<RankEntry> GetUser(string boardId, long gamerTag)
      {
         return GetCache(boardId).Get(gamerTag);
      }

      public Promise<LeaderBoardView> GetBoard(LeaderboardRef leaderBoard, int @from, int max, long? focus = null, long? outlier = null)
         => GetBoard(leaderBoard.Id, from, max, focus, outlier);
      public Promise<LeaderBoardView> GetBoard(string boardId, int @from, int max, long? focus = null, long? outlier = null)
      {
         if(string.IsNullOrEmpty(boardId))
         {
            return Promise<LeaderBoardView>.Failed(new Exception("Leaderboard ID cannot be uninitialized."));
         }
         string query = $"from={from}&max={max}";
         if (focus.HasValue)
         {
            query += $"&focus={focus.Value}";
         }
         if (outlier.HasValue)
         {
            query += $"&outlier={outlier.Value}";
         }

         return Requester.Request<LeaderBoardV2ViewResponse>(
            Method.GET,
            $"/object/leaderboards/{boardId}/view?{query}"
         ).Map(rsp => rsp.lb);
      }

      public Promise<LeaderBoardView> GetRanks(LeaderboardRef leaderBoard, List<long> ids)
         => GetRanks(leaderBoard.Id, ids);

      public Promise<LeaderBoardView> GetRanks(string boardId, List<long> ids)
      {
         var query = "";
         if (ids != null && ids.Count > 0)
         {
            query = $"&ids={string.Join(",", ids)}";
         }

         return Requester.Request<LeaderBoardV2ViewResponse>(
            Method.GET,
            $"/object/leaderboards/{boardId}/ranks?{query}"
         ).Map(rsp => rsp.lb);
      }

      public Promise<EmptyResponse> SetScore(LeaderboardRef leaderBoard, double score, IDictionary<string, object> stats=null)
            => SetScore(leaderBoard.Id, score, stats);


      public Promise<EmptyResponse> SetScore(string boardId, double score, IDictionary<string, object> stats=null)
      {
         return Update(boardId, score, increment: false, stats);
      }

      public Promise<EmptyResponse> IncrementScore(LeaderboardRef leaderBoard, double score, IDictionary<string, object> stats=null)
         => IncrementScore(leaderBoard.Id, score, stats);

      public Promise<EmptyResponse> IncrementScore(string boardId, double score, IDictionary<string, object> stats=null)
      {
         return Update(boardId, score, true, stats);
      }

      protected Promise<EmptyResponse> Update(string boardId, double score, bool increment = false, IDictionary<string, object> stats=null)
      {
         string body = null;
         if (stats != null)
         {
            var req = new ArrayDict
            {
               {"stats", new ArrayDict(stats)}
            };
            body = Json.Serialize(req, new StringBuilder());
         }

         return Requester.Request<EmptyResponse>(
            Method.PUT,
            $"/object/leaderboards/{boardId}/entry?id={UserContext.UserId}&score={score}&increment={increment}", // TODO: move url params into request body.
            body
         ).Then(_ => GetCache(boardId).Remove(UserContext.UserId));
      }

      private Promise<Dictionary<long, RankEntry>> Resolve(string boardId, List<long> gamerTags)
      {
         string queryString = "";
         for (int i = 0; i < gamerTags.Count; i++)
         {
            if (i > 0)
            {
               queryString += ",";
            }

            queryString += gamerTags[i].ToString();
         }

         return Requester.Request<LeaderBoardV2ViewResponse>(
            Method.GET,
            $"/object/leaderboards/{boardId}/ranks?ids={queryString}"
         ).Map(rsp =>
         {
            Dictionary<long, RankEntry> result = new Dictionary<long, RankEntry>();
            var rankings = rsp.lb.ToDictionary();
            for (int i = 0; i < gamerTags.Count; i++)
            {
               RankEntry entry;
               if (!rankings.TryGetValue(gamerTags[i], out entry))
               {
                  entry = new RankEntry();
                  entry.gt = gamerTags[i];
                  entry.columns = new RankEntryColumns();
               }

               result.Add(gamerTags[i], entry);
            }
            return result;
         });
      }

   }
}