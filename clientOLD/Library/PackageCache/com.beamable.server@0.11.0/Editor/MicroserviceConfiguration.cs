using System;
using System.Collections.Generic;
using System.Linq;
using Beamable;
using Beamable.Editor.Environment;
using UnityEngine;

namespace Beamable.Server.Editor
{

   public class MicroserviceConfigConstants : IConfigurationConstants
   {
      public string GetSourcePath(Type type)
      {
         //
         // TODO: make this work for multiple config types
         //       but for now, there is just the one...

         return "Packages/com.beamable.server/Editor/microserviceConfiguration.asset";

      }
   }

   public class MicroserviceConfiguration : AbsModuleConfigurationObject<MicroserviceConfigConstants>
   {

      public static MicroserviceConfiguration Instance => Get<MicroserviceConfiguration>();

      public List<MicroserviceConfigurationEntry> Microservices;

      [Tooltip("When you build a microservice, any ContentType class will automatically be referenced if this field is set to true. Beamable recommends that you put your ContentTypes into a shared assembly definition instead.")]
      public bool AutoReferenceContent = true;

      [Tooltip("When you build and run microservices, the logs will be color coded if this field is set to true.")]
      public bool ColorLogs = true;

      [Tooltip("Docker Buildkit may speed up and increase performance on your microservice builds. However, it is not fully supported with Beamable microservices, and you may encounter issues using it. ")]
      public bool EnableDockerBuildkit = false;

      public Color LogProcessLabelColor = Color.grey;
      public Color LogStandardOutColor = Color.blue;
      public Color LogStandardErrColor = Color.red;
      public Color LogDebugLabelColor =  new Color(.25f, .5f, 1);
      public Color LogInfoLabelColor =  Color.blue;
      public Color LogErrorLabelColor =  Color.red;
      public Color LogWarningLabelColor =  new Color(1, .6f, .15f);
      public Color LogFatalLabelColor =  Color.red;


      #if UNITY_EDITOR
      public override void OnFreshCopy()
      {
         var isDark = UnityEditor.EditorGUIUtility.isProSkin;

         if (isDark)
         {
            LogProcessLabelColor = Color.white;
            LogStandardOutColor = new Color(.2f, .4f, 1f);
            LogStandardErrColor = new Color(1, .44f, .2f);
         }
         else
         {
            LogProcessLabelColor = Color.grey;
            LogStandardOutColor = new Color(.4f, .4f, 1f);
            LogStandardErrColor = new Color(1, .44f, .4f);
         }
      }
      #endif

      public MicroserviceConfigurationEntry GetEntry(string serviceName)
      {
         var existing = Microservices.FirstOrDefault(s => s.ServiceName == serviceName);
         if (existing == null)
         {
            existing = new MicroserviceConfigurationEntry
            {
               ServiceName = serviceName,
               TemplateId = "small",
               Enabled = true,
               DebugData = new MicroserviceConfigurationDebugEntry
               {
                  Password = "Password!",
                  Username = "root",
                  SshPort = 11100 + Microservices.Count
               }
            };
            Microservices.Add(existing);
         }
         return existing;
      }
   }

   [System.Serializable]
   public class MicroserviceConfigurationEntry
   {
      public string ServiceName;
      [Tooltip("If the service should be running on the cloud, in the current realm.")]
      public bool Enabled;
      public string TemplateId;

      public MicroserviceConfigurationDebugEntry DebugData;
   }

   [System.Serializable]
   public class MicroserviceConfigurationDebugEntry
   {
      public string Username = "beamable";
      [Tooltip("The SSH password to use to connect a debugger. This is only supported for local development. SSH is completely disabled on cloud services.")]
      public string Password = "beamable";
      public int SshPort = -1;
   }
}