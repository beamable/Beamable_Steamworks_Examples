using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Beamable.Common
{

   public interface IHasSourcePath
   {
      string SourcePath { get; }
   }

   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
   public class AgnosticAttribute : Attribute, IHasSourcePath
   {
      public Type[] SupportTypes { get; }
      public string SourcePath { get; }
      public string MemberName { get; }

      public AgnosticAttribute(Type[] supportTypes=null, [CallerFilePath] string sourcePath = "", [CallerMemberName] string memberName = "")
      {
         SupportTypes = supportTypes;
         SourcePath = sourcePath;
         MemberName = memberName;
      }
   }
}