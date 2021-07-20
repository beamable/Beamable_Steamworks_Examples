using System;
using System.Runtime.CompilerServices;
namespace Beamable.Common.Content
{
   [AttributeUsage(AttributeTargets.Class)]
   public class ContentTypeAttribute : UnityEngine.Scripting.PreserveAttribute, IHasSourcePath
   {
      public string TypeName { get; }
      public string SourcePath { get; }

      public ContentTypeAttribute(string typeName, [CallerFilePath] string sourcePath = "")
      {
         TypeName = typeName;
         SourcePath = sourcePath;
      }
   }

   [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
   public class ContentFormerlySerializedAsAttribute : Attribute
   {
      public string OldTypeName { get; }
      public ContentFormerlySerializedAsAttribute(string oldTypeName)
      {
         OldTypeName = oldTypeName;
      }
   }
}
