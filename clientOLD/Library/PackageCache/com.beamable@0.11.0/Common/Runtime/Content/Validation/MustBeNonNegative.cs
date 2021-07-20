namespace Beamable.Common.Content.Validation
{
   public class MustBeNonNegative : MustBePositive
   {
      public MustBeNonNegative() : base(allowZero:true){}
   }
}