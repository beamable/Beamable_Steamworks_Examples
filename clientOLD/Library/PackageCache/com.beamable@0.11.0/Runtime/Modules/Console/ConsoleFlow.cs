using System;
using System.Collections.Generic;
using System.Text;
using Beamable.ConsoleCommands;
using Beamable.InputManagerIntegration;
using Beamable.Service;
using UnityEngine;
using UnityEngine.UI;
namespace Beamable.Console
{
   [HelpURL(BeamableConstants.URL_FEATURE_ADMIN_FLOW)]
   public class ConsoleFlow : MonoBehaviour
   {
        private static ConsoleFlow _instance;
        private static readonly Dictionary<string, ConsoleCommand> ConsoleCommandsByName = new Dictionary<string, ConsoleCommand>();

        public Canvas canvas;
        public Text txtOutput;
        public InputField txtInput;



        private bool _isInitialized;
        private bool _showNextTick;
        private bool _isActive;

        private int _fingerCount;
        private bool _waitForRelease;
        private Vector2 _averagePositionStart;

        private IBeamableAPI _beamable;

        private async void Start()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            ServiceManager.ProvideWithDefaultContainer(new BeamableConsole());

            // We want to ensure that we create the instance of the Beamable API if the console is the only thing
            // in the scene.
            _beamable = await API.Instance;

            var console = ServiceManager.Resolve<BeamableConsole>();
            console.OnLog += Log;
            console.OnExecute += ExecuteCommand;
            console.OnCommandRegistered += RegisterCommand;
            try
            {
                console.LoadCommands();
            }
            catch (Exception)
            {
                Debug.LogError("Unable to load console commands.");
            }

            txtInput.onEndEdit.AddListener(evt =>
            {
                if (txtInput.text.Length > 0)
                {
                    Execute(txtInput.text);
                }
            });

            _isInitialized = true;
        }

        private void Awake()
        {
            HideConsole();
        }

        void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            if (_showNextTick)
            {
                DoShow();
                _showNextTick = false;
            }

            if (ConsoleShouldToggle() && ConsoleIsEnabled())
            {
                ToggleConsole();
            }
        }

        /// <summary>
        /// Console should toggle if the toggle key was pressed OR a 3 finger swipe occurred on device.
        /// </summary>
        private bool ConsoleShouldToggle()
        {
            var shouldToggle = BeamableInput.IsActionTriggered(ConsoleConfiguration.Instance.ToggleAction);

#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (shouldToggle)
            {
                // Early out if we already know we must toggle.
                return true;
            }

            var fingerCount = 0;
            var averagePosition = Vector2.zero;

            var touchCount = Input.touchCount;
            for (var i = 0; i < touchCount; ++i)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    fingerCount++;
                    averagePosition += touch.position;
                }
            }

            switch (fingerCount)
            {
                case 3 when !_waitForRelease:
                {
                    averagePosition /= 3;
                    if (_fingerCount != 3)
                    {
                        _averagePositionStart = averagePosition;
                    }
                    else
                    {
                        if ((_averagePositionStart - averagePosition).magnitude > 20.0f)
                        {
                            _waitForRelease = true;
                            shouldToggle = true;
                        }
                    }

                    break;
                }
                case 0 when _waitForRelease:
                    _waitForRelease = false;
                    break;
            }

            _fingerCount = fingerCount;
#endif
            return shouldToggle;
        }

        private bool ConsoleIsEnabled()
        {
#if UNITY_EDITOR
            return true;
#else
            return ConsoleConfiguration.Instance.ForceEnabled || _beamable.User.HasScope("cli:console");
#endif
        }

        private void Execute(string txt)
        {
            if (!_isActive)
            {
                return;
            }

            var parts = txt.Split(' ');
            txtInput.text = "";
            txtInput.Select();
            txtInput.ActivateInputField();
            if (parts.Length == 0)
                return;
            var args = new string[parts.Length - 1];
            for (var i = 1; i < parts.Length; i++)
            {
                args[i - 1] = parts[i];
            }

            Log(ServiceManager.Resolve<BeamableConsole>().Execute(parts[0], args));
        }

        private static void RegisterCommand(BeamableConsoleCommandAttribute command, ConsoleCommandCallback callback)
        {
            foreach (var name in command.Names)
            {
                var cmd = new ConsoleCommand {Command = command, Callback = callback};
                ConsoleCommandsByName[name.ToLower()] = cmd;
            }
        }

        private string ExecuteCommand(string command, string[] args)
        {
            if (command == "help")
            {
                return OnHelp(args);
            }

            if (ConsoleCommandsByName.TryGetValue(command.ToLower(), out var cmd))
            {
                var echoLine = "> " + command;
                foreach (var arg in args)
                {
                    echoLine += " " + arg;
                }

                Log(echoLine);
                return cmd.Callback(args);
            }

            return "Unknown command";
        }

        private string OnHelp(params string[] args)
        {
            if (args.Length == 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine("Listing commands:");
                var uniqueCommands = new HashSet<ConsoleCommand>();
                var commands = ConsoleCommandsByName.Values;
                foreach (var command in commands)
                {
                    if (uniqueCommands.Contains(command))
                    {
                        continue;
                    }

                    uniqueCommands.Add(command);

                    var line = $"{command.Command.Usage} - {command.Command.Description}\n";
                    Debug.Log(line);
                    builder.Append(line);
                }
                return builder.ToString();
            }

            var commandToGetHelpAbout = args[0].ToLower();
            if (ConsoleCommandsByName.TryGetValue(commandToGetHelpAbout, out var found))
            {
                return
                    $"Help information about {commandToGetHelpAbout}\n\tDescription: {found.Command.Description}\n\tUsage: {found.Command.Usage}";
            }

            return $"Cannot find help information about {commandToGetHelpAbout}. Are you sure it is a valid command?";
        }

        private void Log(string line)
        {
            Debug.Log(line);
            txtOutput.text += Environment.NewLine + line;
        }

        public void ToggleConsole()
        {
            if (_isActive)
                HideConsole();
            else
                ShowConsole();
        }

        public void HideConsole()
        {
            _isActive = false;
            txtInput.DeactivateInputField();
            txtInput.text = "";
            canvas.enabled = false;
        }

        public void ShowConsole()
        {
            if (!enabled)
            {
                Debug.LogWarning("Cannot open the console, because it isn't enabled");
                return;
            }

            _showNextTick = true;
        }

        private void DoShow()
        {
            _isActive = true;
            canvas.enabled = true;
            txtInput.text = "";
            txtInput.Select();
            txtInput.ActivateInputField();
        }

        private struct ConsoleCommand
        {
            public BeamableConsoleCommandAttribute Command;
            public ConsoleCommandCallback Callback;
        }
    }
}