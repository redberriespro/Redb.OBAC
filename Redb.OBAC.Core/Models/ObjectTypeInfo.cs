using System;

namespace Redb.OBAC.Core.Models
{
    public class ObjectTypeInfo
    {
        public Guid ObjectTypeId { get; set; }
        public string Description { get; set; }
        public ObjectTypeEnum ObjectType { get; set; }
    }
}