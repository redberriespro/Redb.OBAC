using Redb.OBAC.Core;

namespace Redb.OBAC
{
    public interface IObacConfiguration
    {
        public IObacObjectManager GetObjectManager();
        public IObacPermissionChecker GetPermissionChecker(int currentUserId);
    }
}