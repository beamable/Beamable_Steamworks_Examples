using Beamable.Editor.Content.Models;
using Beamable.Editor.Content;
using UnityEngine;
using Beamable.Editor.UI.Buss.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Editor.Config;
using Beamable.Editor.Content.Components;
using Beamable.Editor.Environment;
using Beamable.Editor.Modules.Theme;
using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.Toolbox.UI.Components;
using Beamable.Editor.UI.Components;
using UnityEditor;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Model
{
    public class MicroserviceModel
    {
        public event Action<MicroserviceModel> OnRenamed;

        // Chris took these out because they weren't being used yet, and were throwing warnings on package builds.
        // public event Action OnRenameRequested;
        // public event Action<MicroserviceModel> OnEnriched;
        private string _name = "";

        public string Name
        {
            set
            {
                if (string.Equals(_name, value)) return;
                if (string.IsNullOrWhiteSpace(value)) throw new Exception("Name cannot be empty.");
                var oldName = _name;
                try
                {
                    _name = value;
                    OnRenamed?.Invoke(this);
                }
                catch (Exception)
                {
                    _name = oldName; // clean up the name
                    throw;
                }
            }
            get { return _name; }
        }
    }


}
