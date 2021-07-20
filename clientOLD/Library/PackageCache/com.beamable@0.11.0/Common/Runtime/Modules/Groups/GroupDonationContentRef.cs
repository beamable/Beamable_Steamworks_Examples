using Beamable.Common.Content;

namespace Beamable.Common.Groups
{
   [System.Serializable]
   public class GroupDonationContentRef<TContent> : ContentRef<TContent> where TContent : GroupDonationsContent, new()
   {

   }
   [System.Serializable]
   public class GroupDonationContentRef : GroupDonationContentRef<GroupDonationsContent>{}
}