using System;
using System.Collections.Generic;
using Redb.OBAC.Core.Hierarchy;
using Redb.OBAC.Models;

namespace Redb.OBAC.Backends
{
    public interface IObacCacheBackend
    {
        void InvalidateForUser(int userId, Guid? objectTypeId = null, Guid? objectId = null);
        void InvalidateForUserGroup(int groupId);
        public void InvalidatePermissionsForObject(Guid objectTypeId, Guid? objectId = null);


        //void SetPermission(int userId, Guid objectTypeId, int? objectId, Guid permission);
        Guid[] GetPermissionsFor(int userId, Guid objectTypeId, Guid? objectId);


        void SetPermissions(int userId, Guid objectTypeId, Guid? objectId, Guid[] permissionIds);
        
        void SetUserId(SubjectInfo subjectInfo);

        SubjectInfo GetUserById(int userId);
        SubjectInfo GetUserByExternalStringId(string extId);
        SubjectInfo GetUserByExternalIntId(in int extId);
        
        void SetGroupId(SubjectInfo subjectInfo);

        SubjectInfo GetGroupById(int groupId);
        SubjectInfo GetGroupByExternalStringId(string extId);
        SubjectInfo GetGroupByExternalIntId(in int extId);
        
    }
}