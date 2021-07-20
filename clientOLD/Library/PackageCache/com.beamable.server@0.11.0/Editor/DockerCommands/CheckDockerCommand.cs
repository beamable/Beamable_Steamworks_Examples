using System;

namespace Beamable.Server.Editor.DockerCommands
{
   public class CheckDockerCommand : DockerCommandReturnable<bool>
   {
      public override string GetCommandString()
      {
         ClearDockerInstallFlag();
         var command = $"{DOCKER_LOCATION} --version";
         return command;
      }

      protected override void Resolve()
      {
         var isInstalled = _exitCode == 0;
         DockerNotInstalled = !isInstalled;
         Promise.CompleteSuccess(isInstalled);
      }
   }
}