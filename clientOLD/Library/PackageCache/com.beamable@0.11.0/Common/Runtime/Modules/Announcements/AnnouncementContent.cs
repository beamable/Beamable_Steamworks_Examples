using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Shop;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beamable.Common.Announcements
{
   [ContentType("announcements")]
   [System.Serializable]
   [Agnostic]
   public class AnnouncementContent : ContentObject
   {
      [Tooltip("The channel is the category of the announcement.")]
      [CannotBeBlank]
      public string channel = "main";

      [Tooltip("The title of the announcement.")]
      [CannotBeBlank]
      public string title = "title";

      [Tooltip("A summary of the announcement.")]
      [CannotBeBlank]
      public string summary = "summary";

      [Tooltip("A main body of the announcement.")]
      [CannotBeBlank]
      public string body = "body";

      [Tooltip("The startDate specifies when the announcement becomes available for players to see. If no startDate is specified, the announcement will become visible immediately. ")]
      [FormerlySerializedAs("start_date")]
      [MustBeDateString]
      [ContentField("start_date")]
      public OptionalString startDate;

      [Tooltip("The endDate specifies when the announcement stops being available for players to see. If no endDate is specified, the announcement will be visible forever. ")]
      [FormerlySerializedAs("end_date")]
      [MustBeDateString]
      [ContentField("end_date")]
      public OptionalString endDate;

      [Tooltip("Attachments can include content that players can claim.")]
      public List<AnnouncementAttachment> attachments;

      [Tooltip("If specified, stat requirements will limit the audience of this announcement based on player stats.")]
      [ContentField("stat_requirements")]
      public OptionalStats statRequirements;

      public OptionalSerializableDictionaryStringToString clientData;
   }
}
