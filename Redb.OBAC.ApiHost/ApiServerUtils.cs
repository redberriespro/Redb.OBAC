using System;
using Google.Protobuf;
using Redberries.OBAC.Api;

namespace Redb.OBAC.ApiHost
{
    public static class ApiServerUtils
    {
        public static Guid ToGuid(this UUID value) => new Guid(value.Value.ToByteArray());
        
        public static UUID ToGrpcUuid(this Guid value)
        {
            return new UUID
            {
                Value = ByteString.CopyFrom(value.ToByteArray())
            };
        }
    }
}