using Beamable.Common.Content;


namespace Beamable.Common.Leaderboards
{
    [ContentType("leaderboards")]
    [System.Serializable]
    [Agnostic]
    public class LeaderboardContent : ContentObject
    {
        public ClientPermissions permissions;
    }
}