namespace Beamable.Common.Content.Validation
{
   public class MustBeComparatorString : MustBeOneOf
   {
      public const string EQUALS = "eq";
      public const string NOT_EQUALS = "ne";
      public const string GREATER_THAN = "gt";
      public const string GREATER_THAN_OR_EQUAL = "ge";
      public const string LESS_THAN = "lt";
      public const string LESS_THAN_OR_EQUAL = "le";
      public MustBeComparatorString() : base(EQUALS, NOT_EQUALS, GREATER_THAN, GREATER_THAN_OR_EQUAL, LESS_THAN, LESS_THAN_OR_EQUAL)
      {

      }
   }
}