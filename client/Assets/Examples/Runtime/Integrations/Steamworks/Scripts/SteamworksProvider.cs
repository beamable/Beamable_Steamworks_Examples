using UnityEngine;
#if USE_STEAMWORKS
using Beamable.Common.Steam;
using Beamable.Service;
using Steamworks;
#endif

namespace Beamable.Examples.Integrations.Steamworks
{
    /// <summary>
    /// Provider for <see cref="Steamworks"/>.
    /// </summary>
    public class SteamworksProvider : MonoBehaviour
    {
        
#if USE_STEAMWORKS
        //  Unity Methods  --------------------------------
        private void Awake()
        {
            ServiceManager.ProvideWithDefaultContainer<ISteamService>(new SteamworksService());
            DontDestroyOnLoad(this.gameObject);
        }
            
        public void Start()
        {
            if(SteamManager.Initialized)
            {
                string personaName = SteamFriends.GetPersonaName();
                var appId = SteamUtils.GetAppID();
                Debug.Log($"Steam User Name = {personaName}, Steam App ID = {appId.m_AppId}");
            }
        }
#endif
        
    }
}
