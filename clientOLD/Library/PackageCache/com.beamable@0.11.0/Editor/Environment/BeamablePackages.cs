using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Beamable.Common;
using UnityEditor;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Beamable.Editor.Environment
{
   public class BeamablePackageMeta
   {
      public bool IsPackageAvailable;
      public string VersionNumber;
   }

   public static class BeamablePackages
   {
      public const string BeamablePackageName = "com.beamable";
      public const string ServerPackageName = "com.beamable.server";

      private static Dictionary<string, Action> _packageToWindowInitialization = new Dictionary<string,Action>();

      public static void ShowServerWindow()
      {
         if (!_packageToWindowInitialization.TryGetValue(ServerPackageName, out var windowInitializer))
         {
            BeamableLogger.LogError("Beamable server not installed.");
         }

         windowInitializer?.Invoke();
      }

      public static Promise<PackageInfo> GetPackageInfo(string packageName)
      {
         var listReq = Client.List(true);
         var promise = new Promise<PackageInfo>();

         void Check()
         {
            if (!listReq.IsCompleted)
            {
               EditorApplication.delayCall += Check;
               return;
            }

            var isSuccess = listReq.Status == StatusCode.Success;
            if (!isSuccess)
            {
               promise.CompleteError(new Exception("Unable to list local packages. " + listReq.Error.message));
            }

            var package = listReq.Result.FirstOrDefault(p => p.name.Equals(packageName));
            promise.CompleteSuccess(package);
         }

         EditorApplication.delayCall += Check;
         return promise;
      }


      public static Promise<BeamablePackageMeta> GetServerPackage()
      {
         var req = Client.List(true);
         var promise = new Promise<BeamablePackageMeta>();

         void Callback()
         {
            if (!req.IsCompleted) return;
            EditorApplication.update -= Callback;

            if (req.Status == StatusCode.Success)
            {
               PackageInfo beamablePackage = null;
               PackageInfo serverPackage = null;
               foreach (var package in req.Result)
               {
                  switch (package.name)
                  {
                     case BeamablePackageName:
                        beamablePackage = package;
                        break;
                     case ServerPackageName:
                        serverPackage = package;
                        break;
                  }
               }

               if (beamablePackage == null)
               {
                  promise.CompleteError(new Exception("no beamable package found"));
                  return;
               }
               if (serverPackage == null)
               {
                  promise.CompleteSuccess(new BeamablePackageMeta
                  {
                     IsPackageAvailable = false,
                     VersionNumber = beamablePackage.version
                  });
               }
               else
               {
                  if (beamablePackage.version != serverPackage.version)
                  {
                     promise.CompleteError(new Exception($"Beamable and Beamable Server need to be at the same version to work. Go to the Unity package manager and resolve the issue. server=[{serverPackage.version}] beamable=[{beamablePackage.version}]"));
                     return;
                  }
                  promise.CompleteSuccess(new BeamablePackageMeta
                  {
                     IsPackageAvailable = true,
                     VersionNumber = serverPackage.version
                  });
               }
            }
            else if (req.Status >= StatusCode.Failure)
            {
               promise.CompleteError(new Exception(req.Error.message));
               BeamableLogger.Log(req.Error.message);
            }

         }

         EditorApplication.update += Callback;
         return promise;
      }

      public static void ProvideServerWindow(Action windowInitializer)
      {
         ProvidePackageWindow(ServerPackageName, windowInitializer);
      }
      public static void ProvidePackageWindow(string packageName, Action windowInitializer)
      {
         _packageToWindowInitialization.Add(packageName, windowInitializer);
      }

      public static Promise<Unit> DownloadServer(BeamablePackageMeta beamableVersion)
      {
         var req = Client.Add($"{ServerPackageName}@{beamableVersion.VersionNumber}");
         var promise = new Promise<Unit>();

         void Callback()
         {
            if (!req.IsCompleted) return;

            EditorApplication.update -= Callback;

            if (req.Status == StatusCode.Success)
            {
               promise.CompleteSuccess(PromiseBase.Unit);
            }
            else if (req.Status >= StatusCode.Failure)
            {
               promise.CompleteError(new Exception(req.Error.message));
               BeamableLogger.Log(req.Error.message);
            }

         }

         EditorApplication.update += Callback;
         return promise;
      }
   }
}