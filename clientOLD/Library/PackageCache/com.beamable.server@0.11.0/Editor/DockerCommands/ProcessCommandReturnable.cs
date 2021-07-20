using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Platform.SDK;
using Beamable.Editor;
using UnityEditor;

namespace Beamable.Server.Editor.DockerCommands
{
   public abstract class DockerCommandReturnable<T> : DockerCommand
   {
      private CommandRunnerWindow _context;
      protected Promise<T> Promise { get; private set; }

      protected string StandardOutBuffer { get; private set; }
      protected string StandardErrorBuffer { get; private set; }

      protected bool _finished;

      public new Promise<T> Start()
      {
         throw new Exception("Not implemented. Must use context overload.");
      }
      public Promise<T> Start(CommandRunnerWindow context)
      {
         if (DockerNotInstalled)
         {
            return Promise<T>.Failed(new DockerNotInstalledException());
         }
         _context = context;
         Promise = new Promise<T>();
         base.Start();

         ForceContextUpdateOnFinish();
         return Promise;
      }

      private void ForceContextUpdateOnFinish()
      {
         void Check()
         {
            if (!_finished) return;
            try
            {
               _context.ForceProcess();
               EditorUtility.SetDirty(_context);
               _context.Repaint();
            }
            finally
            {
               EditorApplication.update -= Check;
            }
         }

         EditorApplication.update += Check;
      }

      protected abstract void Resolve();

      protected override void HandleStandardOut(string data)
      {
         base.HandleStandardOut(data);
         if (data != null)
            StandardOutBuffer += data;
      }

      protected override void HandleStandardErr(string data)
      {
         base.HandleStandardErr(data);
         if (data != null)
            StandardErrorBuffer += data;
      }

      protected override void HandleOnExit()
      {
         _context.RunOnMainThread(() =>
         {
            base.HandleOnExit();
            Resolve();
         });
         _finished = true;
      }
   }

   public class CommandRunnerWindow : EditorWindow
   {
      static CommandRunnerWindow _instance;

      static volatile bool _queued = false;
      static List<Action> _backlog = new List<Action>(8);
      static List<Action> _actions = new List<Action>(8);

      private void Update()
      {
         // this is running on the main thread...
         if (_queued)
         {
            ForceProcess();
         }
      }

      public void ForceProcess()
      {
         lock(_backlog) {
            var tmp = _actions;
            _actions = _backlog;
            _backlog = tmp;
            _queued = false;
         }

         foreach(var action in _actions)
            action();

         _actions.Clear();
      }

      public void RunOnMainThread(Action action)
      {
         lock(_backlog) {
            _backlog.Add(action);
            _queued = true;
         }
      }
   }

}