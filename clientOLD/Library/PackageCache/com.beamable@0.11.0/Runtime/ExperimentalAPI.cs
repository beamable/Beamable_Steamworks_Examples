using Beamable.Api;
using Beamable.Experimental.Api.Chat;
using Beamable.Experimental.Api.Matchmaking;
using Beamable.Experimental.Api.Sim;
using Beamable.Experimental.Api.Calendars;

namespace Beamable.Experimental
{
   /// <summary>
   /// This interface defines the main entry point for the %experimental (labs) features.
   /// 
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   /// 
   /// #### Related Links
   /// - See the <a target="_blank" href="https://docs.beamable.com/docs/beamable-labs-overview">Labs</a> feature documentation
   /// - See Beamable.API script reference
   /// 
   /// ![img beamable-logo]
   /// 
   /// </summary>
   public interface IExperimentalAPI
   {
      ChatService ChatService { get; }
      GameRelayService GameRelayService { get; }
      MatchmakingService MatchmakingService { get; }

      CalendarsService CalendarService { get; }

   }

   /// <summary>
   /// This class defines the main entry point for the %experimental (labs) features.
   /// 
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   /// 
   /// #### Related Links
   /// - See the <a target="_blank" href="https://docs.beamable.com/docs/beamable-labs-overview">Labs</a> feature documentation
   /// - See Beamable.API script reference
   /// 
   /// ![img beamable-logo]
   /// 
   /// </summary>
   public class ExperimentalAPI : IExperimentalAPI
   {
      private readonly PlatformService _platform;
      public ChatService ChatService => _platform.Chat;
      public GameRelayService GameRelayService => _platform.GameRelay;
      public MatchmakingService MatchmakingService => _platform.Matchmaking;
      public CalendarsService CalendarService => _platform.Calendars;

      public ExperimentalAPI(PlatformService platform )
      {
         _platform = platform;
      }
   }
}