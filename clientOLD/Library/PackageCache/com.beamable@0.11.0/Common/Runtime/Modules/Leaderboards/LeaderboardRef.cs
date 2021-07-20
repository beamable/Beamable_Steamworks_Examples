using Beamable.Common.Content;

namespace Beamable.Common.Leaderboards
{
   [System.Serializable]
   [Agnostic]
   public class LeaderboardRef : LeaderboardRef<LeaderboardContent> {}

   [System.Serializable]
   [Agnostic]
   public class LeaderboardRef<TContent> : ContentRef<TContent> where TContent:LeaderboardContent, new() {}
}