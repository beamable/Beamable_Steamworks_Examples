using Beamable.Common.Content;

namespace Beamable.Experimental.Common.Calendars
{
   [System.Serializable]
   public class CalendarRef : CalendarRef<CalendarContent>{} //ContentRef<CalendarContent> {}

   [System.Serializable]
   public class CalendarRef<TContent> : ContentRef<TContent> where TContent:CalendarContent, new()
   {
   }
}