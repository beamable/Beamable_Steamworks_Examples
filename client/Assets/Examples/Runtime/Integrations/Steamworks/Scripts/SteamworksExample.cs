using UnityEngine;
using Steamworks;

namespace Beamable.Examples.Integrations.Steamworks
{
    /// <summary>
    /// Demonstrates <see cref="Steamworks"/>.
    /// </summary>
    public class SteamworksExample : MonoBehaviour
    {
        //  Unity Methods  --------------------------------
        protected void Start()
        {
            Debug.Log($"Start() Instructions...\n" + 
                      " * Complete steps: https://docs.beamable.com/docs/integrating-steamworks\n" + 
                      " * Run The Scene\n" +
                      " * See Unity Console Window for success\n" + 
                      " * See an error? Repeat these steps\n");

            SetupBeamable();
        }
        
        //  Methods  --------------------------------------
        private async void SetupBeamable()
        {
            var beamContext = BeamContext.Default;
            await beamContext.OnReady;
            Debug.Log($"beamContext.PlayerId = {beamContext.PlayerId}");
       
            if(SteamManager.Initialized) 
            {
                // Successfully fetch arbitrary Steamworks data
                string personaName = SteamFriends.GetPersonaName();
                Debug.Log($"Success! SteamFriends.GetPersonaName = {personaName}");
            }
            else
            {
                Debug.Log($"Failure! SteamManager.Initialized = {SteamManager.Initialized}");
            }
        }
    }
}