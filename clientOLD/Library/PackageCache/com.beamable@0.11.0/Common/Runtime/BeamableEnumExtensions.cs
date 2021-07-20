using System;

namespace Beamable.Common
{
   public static class BeamableEnumExtensions
   {
      public static bool ContainsAnyFlag<TEnum>(this TEnum self, TEnum flag) where TEnum : Enum
      {
         var selfInt = Convert.ToInt32(self);
         var flagInt = Convert.ToInt32(flag);
         return selfInt == -1 || (0 < (selfInt & flagInt));
      }

      public static bool ContainsAllFlags<TEnum>(this TEnum self, TEnum flags) where TEnum : Enum
      {
         var selfInt = Convert.ToInt32(self);
         var flagsInt = Convert.ToInt32(flags);

         var isEverything = selfInt == -1;
         var containsAnd = selfInt & flagsInt;
         var hasAll = flagsInt == containsAnd;
         return hasAll || isEverything;
      }
   }
}