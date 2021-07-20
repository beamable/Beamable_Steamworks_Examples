using System.Collections.Generic;

namespace Beamable.Common.Api.CloudData
{
   /// <summary>
   /// This interface defines the main entry point for the %CloudData feature.
   /// 
   /// [img beamable-logo]: https://landen.imgix.net/7udgo2lvquge/assets/xgh89bz1.png?w=400 "Beamable Logo"
   /// 
   /// #### Related Links
   /// - See the <a target="_blank" href="https://docs.beamable.com/docs/cloud-save-feature">Cloud Save</a> feature documentation
   /// - See Beamable.API script reference
   /// 
   /// ![img beamable-logo]
   /// 
   /// </summary>
   public interface ICloudDataApi
   {
      Promise<GetCloudDataManifestResponse> GetGameManifest ();
      Promise<GetCloudDataManifestResponse> GetPlayerManifest ();
   }
}