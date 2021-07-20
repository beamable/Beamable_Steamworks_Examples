using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Beamable.Config;
using Beamable.Editor.Environment;
using Beamable.Serialization.SmallerJSON;
using UnityEngine;

namespace Beamable.Server.Editor.DockerCommands
{

   public class RunImageCommand : DockerCommand
   {
      public int DebugPort { get; }
      public string ImageName { get; set; }
      public string ContainerName { get; set; }

      public string Cid { get; }
      private string Secret { get; set; }
      public string LogLevel { get; }

      public RunImageCommand(MicroserviceDescriptor descriptor, string cid, string secret, string logLevel="Information")
      {
         ImageName = descriptor.ImageName;
         ContainerName = descriptor.ContainerName;
         Cid = cid;
         Secret = secret;
         LogLevel = logLevel;
         DebugPort = MicroserviceConfiguration.Instance.GetEntry(descriptor.Name).DebugData.SshPort;

         UnityLogLabel = $"Docker Run {descriptor.Name}";
      }

      protected override void HandleStandardOut(string data)
      {
         if (!MicroserviceLogHelper.HandleLog(UnityLogLabel, data))
         {
            base.HandleStandardOut(data);
         }
      }

      public override string GetCommandString()
      {
         var pid = ConfigDatabase.GetString("pid");

         var namePrefix = MicroserviceIndividualization.Prefix;
         var command = $"{DOCKER_LOCATION} run --rm " +
                          $"-P " +
                          $"-p {DebugPort}:2222  " +
                          $"--env CID={Cid} " +
                          $"--env PID={pid} " +
                          $"--env SECRET=\"{Secret}\" " +
                          $"--env HOST=\"{BeamableEnvironment.SocketUrl}\" " +
                          $"--env LOG_LEVEL=\"{LogLevel}\" " +
                          $"--env NAME_PREFIX=\"{namePrefix}\" " +

                          $"--name {ContainerName} {ImageName}";


         return command;
      }

   }
}