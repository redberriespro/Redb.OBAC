using System;
using System.Threading.Tasks;

namespace Redb.OBAC.Core
{
    public interface IObacPermissionChecker
    {
        /// <summary>
        /// return list of effective permissions given for the specified Object Type or Object
        /// </summary>
        /// <param name="objectTypeId">Object Type</param>
        /// <param name="objectId">ID of Object (if set). If not set, the object type class permissions will be checked</param>
        /// <returns>list of effective permissions</returns>
        Task<Guid[]> GetObjectPermissions(Guid objectTypeId, Guid? objectId=null);
        
        /// <summary>
        /// Check if current user has specified effective permissions to the certain object 
        /// </summary>
        /// <returns>true if user have permissions</returns>
        Task<bool> CheckObjectPermissions(Guid objectTypeId, Guid objectId, Guid permissionId);
        
        /// <summary>
        /// Check if current user has specified effective permissions to the certain object 
        /// </summary>
        /// <returns>true if user has all the specified permissions</returns>
        Task<bool> CheckObjectPermissions(Guid objectTypeId, Guid objectId, Guid[] permissionId);
    }
}