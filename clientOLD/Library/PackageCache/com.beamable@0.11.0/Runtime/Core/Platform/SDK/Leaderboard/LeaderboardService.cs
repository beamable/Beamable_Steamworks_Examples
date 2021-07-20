﻿using Beamable.Common.Api;
using Beamable.Common.Api.Leaderboards;

namespace Beamable.Api.Leaderboard
{
   /// <summary>
   /// This class defines the main entry point for the %Leaderboards feature.
   /// 
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   /// 
   /// #### Related Links
   /// - See the <a target="_blank" href="https://docs.beamable.com/docs/leaderboards-feature">Leaderboards</a> feature documentation
   /// - See Beamable.API script reference
   /// 
   /// ![img beamable-logo]
   /// 
   /// </summary>
   public class LeaderboardService : LeaderboardApi
   {
      public LeaderboardService(PlatformService platform, IBeamableRequester requester,
         UserDataCache<RankEntry>.FactoryFunction cacheFactory)
         : base(requester, platform, cacheFactory)
      {
      }

      /*
       * Client specific API calls could be here. API calls that the server _shouldn't_ have.
       */
   }
}