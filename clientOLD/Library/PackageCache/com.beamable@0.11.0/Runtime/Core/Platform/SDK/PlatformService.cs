using System;
using System.Collections;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Config;
using Beamable.Coroutines;
using Beamable.Api.Analytics;
using Beamable.Api.Announcements;
using Beamable.Api.Auth;
using Beamable.Api.Connectivity;
using Beamable.Experimental.Api.Chat;
using Beamable.Api.CloudSaving;
using Beamable.Api.Commerce;
using Beamable.Api.Events;
using Beamable.Api.Groups;
using Beamable.Api.Inventory;
using Beamable.Api.Leaderboard;
using Beamable.Api.Mail;
using Beamable.Api.Notification;
using Beamable.Api.Payments;
using Beamable.Api.Sessions;
using Beamable.Api.Stats;
using Beamable.Api.Tournaments;
using Beamable.Api.CloudData;
using Beamable.Common.Api;
using Beamable.Common.Api.Auth;
using Beamable.Common.Api.Content;
using Beamable.Common.Api.Leaderboards;
using Beamable.Content;
using Beamable.Experimental.Api.Calendars;
using Beamable.Experimental.Api.Matchmaking;
using Beamable.Experimental.Api.Sim;
using Beamable.Service;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Beamable.Api
{
   /// <summary>
   /// This interface defines core %Beamable %Service %API.
   /// 
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   /// 
   /// #### Related Links
   /// - See Beamable.Api script reference
   ///
   /// ![img beamable-logo]
   ///
   /// </summary>
   public interface IPlatformService : IUserContext
   {
      // XXX: This is a small subset of the PlatformService, only pulled as needed for testing purposes.

      User User { get; }
      Promise<Unit> OnReady { get; }
      event Action OnShutdown;
      event Action OnReloadUser;
      event Action TimeOverrideChanged;

      NotificationService Notification { get; }

      IConnectivityService ConnectivityService { get; }
   }

   /// <summary>
   /// This class defines core %Beamable %Service %API.
   /// 
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   /// 
   /// #### Related Links
   /// - See Beamable.Api script reference
   ///
   /// ![img beamable-logo]
   ///
   /// </summary>
   [EditorServiceResolver(typeof(PlatformEditorServiceResolver))]
   public class PlatformService : IDisposable, IPlatformService
   {
      private const string acceptHeader = "application/json";
      private const int HeartbeatInterval = 30;
      private const int MaxInitRetries = 4;

      private delegate Promise<Unit> InitStep();

      // Initialization Control
      public Promise<Unit> OnReady { get; set; }
      private InitStep[] _initSteps;
      private int _nextInitStep = 0;
      public event Action OnShutdown;
      public event Action OnReloadUser;

      // Required runtime singletons
      private static AccessTokenStorage _accessTokenStorage = new AccessTokenStorage();

      // API Services
      public AnnouncementsService Announcements;
      public AuthService Auth;
      public CalendarsService Calendars;
      public readonly ChatService Chat;
      public CloudSavingService CloudSaving;

      public IConnectivityService ConnectivityService { get; set; }
      public CommerceService Commerce;
      public EventsService Events;
      public GameRelayService GameRelay;
      public GroupsService Groups;
      public Heartbeat Heartbeat;
      public InventoryService Inventory;
      public LeaderboardService Leaderboard;
      public MailService Mail;
      public MatchmakingService Matchmaking;
      public NotificationService Notification { get; set; }
      public PaymentService Payments;
      public TournamentService Tournaments;
      public ContentService ContentService;
      public CloudDataService CloudDataService;
      public IBeamablePurchaser BeamablePurchaser => ServiceManager.ResolveIfAvailable<IBeamablePurchaser>();
      public Promise<IBeamablePurchaser> InitializedBeamableIAP = new Promise<IBeamablePurchaser>();
      public readonly PubnubNotificationService PubnubNotificationService;
      public PushService Push;
      public SessionService Session;
      public StatsService Stats;

      // High order functionality
      private User _user = new User();
      public AnalyticsTracker Analytics;
      public readonly ChatProvider ChatProvider;
      public PubnubSubscriptionManager PubnubSubscriptionManager;

      // Configuration values
      public bool DebugMode;
      private bool _withLocalNote;

      protected string platform
      {
         get => _requester.Host;
         set => _requester.Host = value;
      }

      public string Cid
      {
         get => _requester.Cid;
         set => _requester.Cid = value;
      }

      public string Pid
      {
         get => _requester.Pid;
         set => _requester.Pid = value;
      }

      public string Shard
      {
         get => _requester.Shard;
         set => _requester.Shard = value;
      }

      public event Action TimeOverrideChanged;

      public string TimeOverride
      {
         get => _requester.TimeOverride;
         set
         {
            if (value == null)
            {
               _requester.TimeOverride = null;
               TimeOverrideChanged();
               return;
            }

            var date = DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var str = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            _requester.TimeOverride = str;
            TimeOverrideChanged();
         }
      }

      // System references
      private GameObject _gameObject;
      private PlatformRequester _requester;

      public User User => _user;
      public PlatformRequester Requester => _requester;

      public AccessToken AccessToken => _requester.Token;

      public PlatformService()
      {

      }

      public PlatformService(bool debugMode, bool withLocalNote = true)
      {
         DebugMode = debugMode;
         _withLocalNote = withLocalNote;

         _gameObject = new GameObject("PlatformService");
         Object.DontDestroyOnLoad(_gameObject);

         // Configure initialization
         OnReady = new Promise<Unit>();
         _initSteps = new InitStep[]
         {
            InitStepLoadToken,
            InitStepRefreshAccount,
            InitStepGetAccount,
            InitStepStartSession,
            InitStepStartAuxiliary
         };

         // Attach child services
         ConnectivityService = new ConnectivityService(ServiceManager.Resolve<CoroutineService>());
         _requester = new PlatformRequester("", _accessTokenStorage, ConnectivityService);
         if (_gameObject != null)
            Notification = _gameObject.AddComponent<NotificationService>();
         Analytics = new AnalyticsTracker(this, _requester, ServiceManager.Resolve<CoroutineService>(), 30, 10);
         Announcements = new AnnouncementsService(this, _requester);
         Auth = new AuthService(_requester);
         _requester.AuthService = Auth;
         Chat = new ChatService(this, _requester);
         if (_gameObject != null)
            ChatProvider = _gameObject.AddComponent<PubNubChatProvider>();
         CloudSaving = new CloudSavingService(this, _requester, ServiceManager.Resolve<CoroutineService>());
         Commerce = new CommerceService(this, _requester);
         Events = new EventsService(this, _requester);
         GameRelay = new GameRelayService(this, _requester);
         Groups = new GroupsService(this, _requester);
         Inventory = new InventoryService(this, _requester);
         Leaderboard = new LeaderboardService(this, _requester, UnityUserDataCache<RankEntry>.CreateInstance);
         Mail = new MailService(this, _requester);
         Matchmaking = new MatchmakingService(this, _requester);
         Payments = new PaymentService(this, _requester);
         PubnubNotificationService = new PubnubNotificationService(_requester);
         if (_gameObject != null)
         {
            PubnubSubscriptionManager = _gameObject.AddComponent<PubnubSubscriptionManager>();
            PubnubSubscriptionManager.Initialize(this);
         }

         Push = new PushService(_requester);
         Session = new SessionService(this, _requester);
         Stats = new StatsService(this, _requester, UnityUserDataCache<Dictionary<string, string>>.CreateInstance);
         Tournaments = new TournamentService(Stats, _requester, this);
         ContentService = new ContentService(this, _requester);
         ContentApi.Instance
            .CompleteSuccess(ContentService); // TODO: This is hacky until we can get the serviceManager into common.
         CloudDataService = new CloudDataService(this, _requester);
      }

      public void Dispose()
      {
         OnShutdown?.Invoke();
         _requester?.Dispose();

         if (ApplicationLifetime.isQuitting)
         {
            return;
         }

         if (_gameObject != null)
            Object.Destroy(_gameObject);
      }

      private void ContinueInitialize(Promise<Unit> initResult)
      {
         if (_nextInitStep >= _initSteps.Length)
         {
            OnReady.CompleteSuccess(PromiseBase.Unit);
            initResult.CompleteSuccess(PromiseBase.Unit);
            return;
         }

         var coroutineService = ServiceManager.Resolve<CoroutineService>();
         coroutineService.StartNew("Platform", RetryInitializeStep(initResult));
      }

      private IEnumerator RetryInitializeStep(Promise<Unit> initResult)
      {
         var tries = 0;
         var done = false;
         Exception lastError = null;
         var skipped = false;
         while (!done && (tries < MaxInitRetries))
         {
            var stepDone = false;
            var promise = _initSteps[_nextInitStep]();
            promise.Then(result =>
            {
               stepDone = true;
               done = true;
            });
            promise.Error(err =>
            {
               if (err is NoConnectivityException)
               {
                  Debug.LogWarning(err.Message);
                  skipped = true;
               }

               lastError = err;
               stepDone = true;
               tries++;
            });

            // Wait for the outstanding promise to resolve
            while (!stepDone)
            {
               yield return Yielders.EndOfFrame;
            }
         }

         if (done && !skipped)
         {
            _nextInitStep += 1;
            ContinueInitialize(initResult);
         }
         else if (skipped && _nextInitStep > Array.IndexOf(_initSteps, InitStepGetAccount))
         {
            OnReady.CompleteSuccess(PromiseBase.Unit);
            initResult.CompleteSuccess(PromiseBase.Unit);
         }
         else
         {
            initResult.CompleteError(lastError);
         }
      }

      private void RetryInitializationOnInternetReconnect(bool tryToRestart)
      {
         if (tryToRestart)
         {
            OnReady = new Promise<Unit>();
            Promise<Unit> initResult = new Promise<Unit>();
            ContinueInitialize(initResult);
         }
      }

      public Promise<Unit> Initialize(string language)
      {
         var initResult = new Promise<Unit>();

         // Pull out config values
         platform = ConfigDatabase.GetString("platform");
         Cid = ConfigDatabase.GetString("cid");
         Pid = ConfigDatabase.GetString("pid");
         _requester.Language = language;
         ConnectivityService.OnConnectivityChanged += RetryInitializationOnInternetReconnect;
         ContinueInitialize(initResult);

         return initResult;
      }

      private Promise<Unit> InitStepLoadToken()
      {
         return _accessTokenStorage.LoadTokenForRealm(Cid, Pid).Map(token =>
         {
            _requester.Token = token;
            return PromiseBase.Unit;
         });
      }

      private Promise<Unit> InitStepRefreshAccount()
      {
         // Create a new account
         if (_requester.Token == null)
         {
            return Auth.CreateUser().Map(rsp =>
            {
               SaveToken(rsp);
               return PromiseBase.Unit;
            });
         }

         // Refresh token
         if (_requester.Token.IsExpired)
         {
            return Auth.LoginRefreshToken(_requester.Token.RefreshToken).Map(rsp =>
            {
               SaveToken(rsp);
               return PromiseBase.Unit;
            }).Recover(ex =>
            {
               if (ex is NoConnectivityException)
               {
                  return PromiseBase.Unit;
               }

               throw ex;
            });
         }

         // Ready
         return Promise<Unit>.Successful(PromiseBase.Unit);
      }

      private Promise<Unit> InitStepGetAccount()
      {
         return ReloadUser().Map(rsp => PromiseBase.Unit);
      }

      public Promise<Unit> StartNewSession()
      {
         return AdvertisingIdentifier.AdvertisingIdentifier.GetIdentifier()
            .FlatMap(id => Session.StartSession(_user, id, _requester.Language)).Map(_ => PromiseBase.Unit);
      }

      private Promise<Unit> InitStepStartSession()
      {
         return StartNewSession();
      }

      private Promise<Unit> InitStepStartAuxiliary()
      {
         //If you lose internet in the middle of these warming up, we may not recover properly.
         BeamablePurchaser?.Initialize().Then(_ => { InitializedBeamableIAP.CompleteSuccess(BeamablePurchaser); });
         if (_withLocalNote)
            Notification.RegisterForNotifications(this);
         Heartbeat = new Heartbeat(this, ServiceManager.Resolve<CoroutineService>(), HeartbeatInterval);
         Heartbeat.Start();
         return Promise<Unit>.Successful(PromiseBase.Unit);
      }

      public Promise<ISet<UserBundle>> GetDeviceUsers()
      {
         var promises = Array.ConvertAll(_accessTokenStorage.RetrieveDeviceRefreshTokens(Cid, Pid),
            token => Auth.GetUser(token).Map(user => new UserBundle
            {
               User = user,
               Token = token
            }));

         return Promise.Sequence(promises)
            .Map(userBundles => (new HashSet<UserBundle>(userBundles) as ISet<UserBundle>));
      }

      public void RemoveDeviceUsers(TokenResponse token)
      {
         _accessTokenStorage.RemoveDeviceRefreshToken(Cid, Pid, token);
      }

      public Promise<User> ReloadUser()
      {
         return Auth.GetUser().Map(user =>
         {
            _user = user;

            // Ensure that we reset the pubnub subscription when we reload the user.
            PubnubSubscriptionManager.UnsubscribeAll();
            PubnubSubscriptionManager.SubscribeToProvider();

            OnReloadUser?.Invoke();
            return user;
         });
      }

      public void SetUser(User user)
      {
         _user = user;
      }

      public Promise<Unit> SaveToken(TokenResponse rsp)
      {
         ClearToken();
         _requester.Token = new AccessToken(_accessTokenStorage, Cid, Pid, rsp.access_token, rsp.refresh_token,
            rsp.expires_in);
         return _requester.Token.Save();
      }

      public void ClearToken()
      {
         _requester.DeleteToken();
      }

      public void ClearDeviceUsers()
      {
         ClearToken();
         _accessTokenStorage.ClearDeviceRefreshTokens(Cid, Pid);
      }

      public long UserId => _user.id;
   }

   public class PlatformEditorServiceResolver : IServiceResolver<PlatformService>
   {
      private static PlatformService instance;

      public bool CanResolve()
      {
         return !ApplicationLifetime.isQuitting;
      }

      public bool Exists()
      {
         return (instance != null) && !ApplicationLifetime.isQuitting;
      }

      public PlatformService Resolve()
      {
         if (instance == null)
         {
            if (ApplicationLifetime.isQuitting)
            {
               return null;
            }

            ConfigDatabase.Init();

            instance = new PlatformService(true);
            instance.Initialize("en");
         }

         return instance;
      }

      public void OnTeardown()
      {
         if (ApplicationLifetime.isQuitting)
         {
            return;
         }

         instance = null;
      }
   }
}