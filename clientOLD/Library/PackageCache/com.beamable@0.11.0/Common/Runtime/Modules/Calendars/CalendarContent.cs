using System;
using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Experimental.Common.Api.Calendars;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beamable.Experimental.Common.Calendars
{
   [ContentType("calendars")]
   public class CalendarContent : ContentObject
   {
      [FormerlySerializedAs("start_date")]
      [MustBeDateString]
      [ContentField("start_date")]
      public OptionalString startDate;

      [HideInInspector] // this is a legacy entitlements setup we don't support. But we can't delete it because scala requires it.
      [Obsolete]
      public List<RewardCalendarDay> days;
   }
}