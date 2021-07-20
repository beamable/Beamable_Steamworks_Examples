using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beamable.Api;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Serialization.SmallerJSON;
using UnityEngine;

namespace Beamable.Experimental.Api.Matchmaking
{
  
  /// <summary>
  /// This class defines the main entry point for the %Matchmaking feature.
  /// 
  /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
  /// 
  /// #### Related Links
  /// - See the <a target="_blank" href="https://docs.beamable.com/docs/multiplayer-matchmaking">Matchmaking</a> feature documentation
  /// - See Beamable.API script reference
  /// 
  /// ![img beamable-logo]
  /// 
  /// </summary>
  public class MatchmakingService
  {
    private readonly IBeamableRequester _requester;
    private readonly IPlatformService _platform;

    public MatchmakingService(IPlatformService platform, IBeamableRequester requester)
    {
      _requester = requester;
      _platform = platform;
    }

    /// <summary>
    /// Initialize the matchmaking process.
    /// </summary>
    /// <param name="gameTypeRef"></param>
    /// <param name="updateHandler"></param>
    /// <param name="readyHandler"></param>
    /// <param name="timeoutHandler"></param>
    /// <returns>A `MatchmakingHandle` which will be updated via push notifications.</returns>
    public Promise<MatchmakingHandle> StartMatchmaking(
      ContentRef<SimGameType> gameTypeRef,
      Action<MatchmakingHandle> updateHandler = null,
      Action<MatchmakingHandle> readyHandler = null,
      Action<MatchmakingHandle> timeoutHandler = null
    )
    {
      return gameTypeRef.Resolve().FlatMap(gameType =>
      {
        TimeSpan? maxWait = null;
        if (gameType.maxWaitDurationSecs.HasValue)
        {
          maxWait = TimeSpan.FromSeconds(gameType.maxWaitDurationSecs.Value);
        }

        return StartMatchmaking(
          gameType.Id,
          updateHandler,
          readyHandler,
          timeoutHandler,
          maxWait
        );
      });
    }

    /// <summary>
    /// Initialize the matchmaking process.
    /// </summary>
    /// <param name="gameType"></param>
    /// <param name="updateHandler"></param>
    /// <param name="readyHandler"></param>
    /// <param name="timeoutHandler"></param>
    /// <param name="maxWait"></param>
    /// <returns>A `MatchmakingHandle` which will be updated via push notifications.</returns>
    public Promise<MatchmakingHandle> StartMatchmaking(
      string gameType,
      Action<MatchmakingHandle> updateHandler = null,
      Action<MatchmakingHandle> readyHandler = null,
      Action<MatchmakingHandle> timeoutHandler = null,
      TimeSpan? maxWait = null
    )
    {
      return MakeMatchmakingRequest(gameType).Map(status => new MatchmakingHandle(
        this,
        _platform,
        gameType,
        status,
        maxWait,
        updateHandler,
        readyHandler,
        timeoutHandler
      ));
    }

    /// <summary>
    /// Cancels matchmaking for the player
    /// </summary>
    /// <param name="gameType">The string id of the game type we wish to be removed from</param>
    public Promise<Unit> CancelMatchmaking(string gameType)
    {
      return _requester.Request<MatchmakingUpdate>(
        Method.DELETE,
        $"/object/matchmaking/{gameType}/match"
      ).Map(_ => PromiseBase.Unit);
    }

    /// <summary>
    /// Find this player a match for the given game type
    /// </summary>
    /// <param name="gameType">The string id of the game type we wish to be matched</param>
    /// <returns></returns>
    private Promise<MatchmakingUpdate> MakeMatchmakingRequest(string gameType)
    {
      return _requester.Request<MatchmakingUpdate>(
        Method.POST,
        $"/object/matchmaking/{gameType}/match"
      );
    }
  }

  public class MatchmakingHandle : IDisposable
  {
    public MatchmakingStatus Status { get; }
    public readonly string GameType;
    public MatchmakingState State { get; private set; }
    public bool MatchmakingIsComplete => State.IsTerminal();

    public event Action<MatchmakingHandle> OnUpdate;
    public event Action<MatchmakingHandle> OnMatchReady;
    public event Action<MatchmakingHandle> OnMatchTimeout;

    private readonly float _createdTime;
    private readonly TimeSpan? _maxWait;

    private readonly IPlatformService _platform;
    private string MessageType => $"matchmaking.update.{GameType}";
    private string TimeoutMessageType => $"matchmaking.timeout.{GameType}";
    private readonly MatchmakingService _service;

    public MatchmakingHandle(
      MatchmakingService service,
      IPlatformService platform,
      string gameType,
      MatchmakingUpdate update,
      TimeSpan? maxWait = null,
      Action<MatchmakingHandle> onUpdate = null,
      Action<MatchmakingHandle> onMatchReady = null,
      Action<MatchmakingHandle> onMatchTimeout = null
    )
    {
      GameType = gameType;
      State = MatchmakingState.Searching;

      Status = new MatchmakingStatus();
      Status.Apply(update);

      OnUpdate = onUpdate;
      OnMatchReady = onMatchReady;
      OnMatchTimeout = onMatchTimeout;

      _platform = platform;
      _maxWait = maxWait;

      _createdTime = Time.realtimeSinceStartup;

      _service = service;

      StartTimeoutTask();
      SubscribeToUpdates();
    }

    public async void Dispose()
    {
      await Cancel();
    }

    /// <summary>
    /// Promise which will complete when the matchmaking client reaches a "resolution".
    /// </summary>
    /// <returns>A promise containing the matchmaking handle itself.</returns>
    public Promise<MatchmakingHandle> WhenCompleted()
    {
      var promise = new Promise<MatchmakingHandle>();
      WaitForComplete(promise);
      return promise;
    }

    private async void WaitForComplete(Promise<MatchmakingHandle> promise)
    {
      var endTime = _createdTime + _maxWait?.TotalSeconds ?? double.MaxValue;
      while (Time.realtimeSinceStartup < endTime)
      {
        if (MatchmakingIsComplete)
        {
          promise.CompleteSuccess(this);
        }

        await Task.Delay(TimeSpan.FromSeconds(1));
      }
    }

    /// <summary>
    /// Cancels matchmaking for this player.
    /// </summary>
    /// <returns>The MatchmakingHandle</returns>
    public Promise<MatchmakingHandle> Cancel()
    {
      State = MatchmakingState.Cancelled;
      _service.CancelMatchmaking(GameType);
      UnsubscribeFromUpdates();
      return Promise<MatchmakingHandle>.Successful(this);
    }

    private async void StartTimeoutTask()
    {
      if (!_maxWait.HasValue)
      {
        return;
      }

      var endTime = _createdTime + _maxWait.Value.TotalSeconds;
      await Task.Delay(TimeSpan.FromSeconds(endTime - Time.realtimeSinceStartup));
      if (MatchmakingIsComplete)
      {
        return;
      }

      // Ensure that we cancel matchmaking if the client is giving up before the server.
      await _service.CancelMatchmaking(GameType);
      ProcessTimeout();
    }

    private void SubscribeToUpdates()
    {
      _platform.Notification.Subscribe(MessageType, OnRawUpdate);
      _platform.Notification.Subscribe(TimeoutMessageType, OnRawTimeout);
    }

    private void UnsubscribeFromUpdates()
    {
      _platform.Notification.Unsubscribe(MessageType, OnRawUpdate);
      _platform.Notification.Unsubscribe(TimeoutMessageType, OnRawTimeout);
    }

    private void OnRawUpdate(object msg)
    {
      // XXX: Ugh. This is an annoying shape to get messages in.
      var serialized = Json.Serialize(msg, new StringBuilder());
      var deserialized = JsonUtility.FromJson<MatchmakingUpdate>(serialized);
      ProcessUpdate(deserialized);
    }

    private void ProcessUpdate(MatchmakingUpdate status)
    {
      Status.Apply(status);

      // Once the game has been marked as "Started" we will no longer receive messages from the server.
      // However, let's ensure that we call OnUpdate regardless in case someone doesn't want to use the
      // OnMatchReady event.
      if (status.gameStarted)
      {
        State = MatchmakingState.Ready;
        try
        {
          OnMatchReady?.Invoke(this);
        }
        catch (Exception e)
        {
          BeamableLogger.LogException(e);
        }
        finally
        {
          UnsubscribeFromUpdates();
        }
      }

      try
      {
        OnUpdate?.Invoke(this);
      }
      catch (Exception e)
      {
        BeamableLogger.LogException(e);
      }
    }

    private void OnRawTimeout(object msg)
    {
      ProcessTimeout();
    }

    private void ProcessTimeout()
    {
      State = MatchmakingState.Timeout;
      try
      {
        OnMatchTimeout?.Invoke(this);
      }
      catch (Exception e)
      {
        BeamableLogger.LogException(e);
      }
      finally
      {
        // Once we get an error, we know that we no longer should receive any updates.
        UnsubscribeFromUpdates();
      }
    }
  }

  public class MatchmakingStatus
  {
    public string GameId { get; private set; }
    public int SecondsRemaining { get; private set; }
    public List<long> Players { get; private set; }
    public bool MinPlayersReached { get; private set; }
    public bool GameStarted { get; private set; }

    public void Apply(MatchmakingUpdate update)
    {
      GameId = update.game;
      SecondsRemaining = update.secondsRemaining;
      Players = update.players;
      MinPlayersReached = update.minPlayersReached;
      GameStarted = update.gameStarted;
    }
  }

  [Serializable]
  public class MatchmakingUpdate
  {
    public string game;
    public int secondsRemaining;
    public List<long> players;
    public bool minPlayersReached;
    public bool gameStarted;
  }

  public enum MatchmakingState
  {
    Searching,
    Ready,
    Timeout,
    Cancelled
  }

  // Define an extension method in a non-nested static class.
  public static class MatchmakingStateExtensions
  {
    public static bool IsTerminal(this MatchmakingState state)
    {
      return state != MatchmakingState.Searching;
    }
  }
}
