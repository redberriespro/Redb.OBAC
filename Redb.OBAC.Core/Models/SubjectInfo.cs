using Redb.OBAC.Core.Models;

namespace Redb.OBAC.Models
{
    public class SubjectInfo
    {
        public int SubjectId { get; set; }
        public SubjectTypeEnum SubjectType { get; set; }
        public string Description { get; set; }
        public int? ExternalIntId { get; set; }
        public string ExternalStringId { get; set; }
    }
}