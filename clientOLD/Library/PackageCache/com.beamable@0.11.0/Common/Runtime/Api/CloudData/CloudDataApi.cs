using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Beamable.Common.Api.CloudData
{
    [Serializable]
    public class GetCloudDataManifestResponse
    {
        public string result;
        public List<CloudMetaData> meta;
    }

    [Serializable]
    public class CloudMetaData
    {
        public long sid;
        public long version;
        public string @ref;
        public string uri;
        public CohortEntry cohort;

        public bool IsDefault => string.IsNullOrEmpty(cohort?.trial) && string.IsNullOrEmpty(cohort?.cohort);
    }

    [Serializable]
    public class CohortEntry
    {
        public string trial;
        public string cohort;
    }

    public class CloudDataApi : ICloudDataApi
    {
        public IUserContext Ctx { get; }
        public IBeamableRequester Requester { get; }

        public CloudDataApi(IUserContext ctx, IBeamableRequester requester)
        {
            Ctx = ctx;
            Requester = requester;
        }

        public Promise<GetCloudDataManifestResponse> GetGameManifest()
        {
            return Requester.Request<GetCloudDataManifestResponse>(
               Method.GET,
               "/basic/cloud/meta"
            );
        }

        public Promise<GetCloudDataManifestResponse> GetPlayerManifest()
        {
            return Requester.Request<GetCloudDataManifestResponse>(
               Method.GET,
               $"/basic/cloud/meta/player/all"
            );
        }
    }
}