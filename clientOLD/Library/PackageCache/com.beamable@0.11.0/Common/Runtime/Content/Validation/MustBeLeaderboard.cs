
using Beamable.Common.Leaderboards;

namespace Beamable.Common.Content.Validation
{
   public class MustBeLeaderboard : MustReferenceContent
   {
      public MustBeLeaderboard(bool allowNull=false) : base(allowNull, allowedTypes:new[] {typeof(LeaderboardContent)})
      {

      }
   }
}