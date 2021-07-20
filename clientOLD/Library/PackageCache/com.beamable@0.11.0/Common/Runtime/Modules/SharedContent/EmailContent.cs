using Beamable.Common.Content.Validation;

namespace Beamable.Common.Content
{
   [ContentType("emails")]
   [System.Serializable]
   [Agnostic]
   public class EmailContent : ContentObject
   {
      [CannotBeBlank]
      public string subject;

      public string body;
   }

   [System.Serializable]
   [Agnostic]
   public class EmailRef : EmailRef<EmailContent> {}

   [System.Serializable]
   public class EmailRef<TContent>: ContentRef<TContent> where TContent : EmailContent, new(){}
}