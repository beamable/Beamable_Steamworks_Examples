using System;
using System.Collections.Generic;
using Beamable.Common.Content.Validation;

namespace Beamable.Common.Content
{
   [ContentType("game_types")]
   [System.Serializable]
   [Agnostic]
   public class SimGameType : ContentObject
   {
      [MustBePositive]
      public int maxPlayers;

      [MustBePositive]
      public OptionalInt minPlayersToStart;

      [MustBePositive]
      public OptionalInt waitAfterMinReachedSecs;

      [MustBePositive]
      public OptionalInt maxWaitDurationSecs;

      public List<LeaderboardUpdate> leaderboardUpdates;
      public List<RewardsPerRank> rewards;
   }

   [Serializable]
   public class SimGameTypeRef : ContentRef<SimGameType>
   {
   }

   [System.Serializable]
   [Agnostic]
   public class RewardsPerRank
   {
      [MustBeNonNegative]
      public int startRank;

      [MustBeNonNegative]
      public int endRank;
      public List<Reward> rewards;
   }

   [System.Serializable]
   [Agnostic]
   public class Reward
   {
      public RewardType type;

      [MustBeCurrency]
      // TODO: This should be a CurrencyRef but the serialization isn't supported on the backend.
      public string name;
      public long amount;
   }

   [System.Serializable]
   [Agnostic]
   public enum RewardType
   {
      Currency
   }

   [System.Serializable]
   [Agnostic]
   public class LeaderboardUpdate
   {
      // TODO: This should be a LeaderboardRef but the serialization isn't supported on the backend.
      [MustBeLeaderboard]
      public string leaderboard;
      public ScoringAlgorithm scoringAlgorithm;
   }

   [System.Serializable]
   [Agnostic]
   public class ScoringAlgorithm
   {
      public AlgorithmType algorithm;
      public List<ScoringAlgoOption> options; // TODO: [MustBeUnique(nameof(ScoringAlgoOption.key))]
   }

   [System.Serializable]
   [Agnostic]
   public class ScoringAlgoOption
   {
      [CannotBeBlank] // TODO: Add [MustBeUniqueInArray]
      public string key;
      public string value;
   }

   [System.Serializable]
   [Agnostic]
   public enum AlgorithmType
   {
      MultiplayerElo
   }
}
