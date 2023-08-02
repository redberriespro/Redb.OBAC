using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Redb.OBAC.ApiClient;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Exceptions;
using Redberries.OBAC.Api;

namespace Redb.OBAC.ApiHost
{
    public class ApiHostImpl : ObacApiHost.ObacApiHostBase
    {
        private readonly IObacConfiguration _configuration;
        private readonly IObacObjectManager _objectManager;
        private ConcurrentDictionary<int,IObacPermissionChecker> _permissionCheckers = new ConcurrentDictionary<int, IObacPermissionChecker>();

        public ApiHostImpl(IObacConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
            _objectManager = configuration.GetObjectManager();
        }

        #region GRPC
        public override async Task<TreeInfoResults> GetTreeById(GetTreeParams request, ServerCallContext context)
        {
            var treeInfo = await _objectManager.GetTree(request.TreeId.ToGuid());
            if (treeInfo == null) return new TreeInfoResults();
            var res = new TreeInfoResults
            {
                TreeId = treeInfo.TreeObjectTypeId.ToGrpcUuid(),
                Description = treeInfo.Description
            };
            if (request.IncludeNodes)
            {
                var nodes = await _objectManager.GetTreeNodes(request.TreeId.ToGuid(),
                    request.StartingNodeId==0?(int?)null:request.StartingNodeId, true);

                foreach (var nd in nodes)
                {
                    res.Nodes.Add(new TreeNodeItemInfo
                    {
                        TreeId = request.TreeId,
                        Id = nd.NodeId,
                        ParentId = nd.ParentNodeId??0
                    });
                }
            }

            return res;
        }

        public override async Task<TreeInfoResults> EnsureTree(EnsureTreeParams request, ServerCallContext context)
        {
            var ct = await _objectManager.EnsureTree(
                request.TreeId.ToGuid(), request.Description,
                request.ExternalIntId switch
                {
                    0 => null,
                    _ => request.ExternalIntId
                },
                  string.IsNullOrEmpty(request.ExternalStrId)?null:request.ExternalStrId);
            
            return new TreeInfoResults
            {
                TreeId = ct.TreeObjectTypeId.ToGrpcUuid(),
                Description = ct.Description
            };
        }
        
        public override async Task<EnsureTreeNodeResults> EnsureTreeNodes(EnsureTreeNodeParams request,
            ServerCallContext context)
        {
            var treeId = request.TreeId.ToGuid();
            foreach (var ni in request.Nodes)
            {
                await _objectManager.EnsureTreeNode(treeId,
                    ni.Id,
                    ni.ParentId switch
                    {
                        0 => null,
                        _ => ni.ParentId
                    },
                    ni.OwnerUserId,
                    ni.ExternalIntId==0?null:ni.ExternalIntId,
                    String.IsNullOrEmpty(ni.ExternalStrId)?null:ni.ExternalStrId
                );
            }

            return new EnsureTreeNodeResults { TreeId = request.TreeId};
        }

        public override async Task<NoResults> DeleteTree(DeleteTreeParams request, ServerCallContext context)
        {
            await _objectManager.DeleteTree(request.TreeId.ToGuid(), request.ForceDeleteIfNotEmpty);
            return new NoResults();
        }

        public override async Task<PermissionsInfoResults> GetPermissions(GetPermissionsParams request,
            ServerCallContext context)
        {
            var perms = (await _objectManager.GetPermissions())
                .Select(p => new PermissionInfoResults
                {
                    PermissionId = p.PermissionId.ToGrpcUuid(),
                    Description = p.Description
                });
            var res = new PermissionsInfoResults();
            res.Permissions.AddRange(perms);

            return res;
        }

        public override async Task<PermissionInfoResults> GetPermissionById(GetPermissionParams request,
            ServerCallContext context)
        {
            var info = await _objectManager.GetPermission(request.PermissionId.ToGuid());
            if (info == null) return new PermissionInfoResults();

            return new PermissionInfoResults
            {
                PermissionId = info.PermissionId.ToGrpcUuid(),
                Description = info.Description
            };
        }

        public override async Task<PermissionInfoResults> EnsurePermission(EnsurePermissionParams request,
            ServerCallContext context)
        {
            await _objectManager.EnsurePermission(
                request.PermissionId.ToGuid(), request.Description, true);

            return new PermissionInfoResults
            {
                PermissionId = request.PermissionId,
                Description = request.Description
            };
        }

        public override async Task<NoResults> DeletePermission(DeletePermissionParams request,
            ServerCallContext context)
        {
            await _objectManager.DeletePermission(request.PermissionId.ToGuid(), request.ForceDelete);
            return new NoResults();
        }


        public override async Task<RoleInfoResults> GetRoleById(GetRoleParams request,
            ServerCallContext context)
        {
            var info = await _objectManager.GetRole(request.RoleId.ToGuid());
            if (info == null) return new RoleInfoResults();

            var res = new RoleInfoResults
            {
                RoleId = info.RoleId.ToGrpcUuid(),
                Description = info.Description
            };

            foreach (var p in info.PermissionIds)
            {
                res.PermissionId.Add(p.ToGrpcUuid());
            }

            return res;
        }

        public override async Task<RoleInfoResults> EnsureRole(EnsureRoleParams request,
            ServerCallContext context)
        {
            var pids = request.PermissionId.Select(s => s.ToGuid()).ToArray();

            await _objectManager.EnsureRole(
                request.RoleId.ToGuid(), request.Description, pids);

            var res = new RoleInfoResults
            {
                RoleId = request.RoleId,
                Description = request.Description
            };
            res.PermissionId.AddRange(request.PermissionId);
            return res;
        }

        public override async Task<NoResults> DeleteRole(DeleteRoleParams request, ServerCallContext context)
        {
            await _objectManager.DeleteRole(request.RoleId.ToGuid());
            return new NoResults();
        }

        public override async Task<UserInfoResults> GetUserById(GetUserParams request,
            ServerCallContext context)
        {
            var info = await _objectManager.GetUser(request.UserId);
            if (info == null) return new UserInfoResults();

            var res = new UserInfoResults
            {
                UserId = info.SubjectId,
                Description = info.Description
            };

            return res;
        }

        public override async Task<UserInfoResults> EnsureUser(EnsureUserParams request,
            ServerCallContext context)
        {
            await _objectManager.EnsureUser(
                request.UserId, request.Description,
                request.ExternalIntId == 0?null:request.ExternalIntId,
                 string.IsNullOrEmpty(request.ExternalStrId)?null:request.ExternalStrId);

            var res = new UserInfoResults
            {
                UserId = request.UserId,
                Description = request.Description
            };
            return res;
        }

        public override async Task<NoResults> DeleteUser(DeleteUserParams request, ServerCallContext context)
        {
            await _objectManager.DeleteUser(request.UserId, request.ForceDelete);
            return new NoResults();
        }

        public override async Task<UserGroupInfoResults> GetUserGroupById(GetUserGroupParams request,
            ServerCallContext context)
        {
            var info = await _objectManager.GetUserGroup(request.UserGroupId);
            if (info == null) return new UserGroupInfoResults();

            var res = new UserGroupInfoResults
            {
                UserGroupId = info.SubjectId,
                Description = info.Description
            };

            return res;
        }

        public override async Task<UserGroupUsersInfoResults> GetUserGroupUsers(GetUserGroupParams request,
            ServerCallContext context)
        {
            var info = await _objectManager.GetUserGroup(request.UserGroupId);
            if (info == null) return new UserGroupUsersInfoResults {UserGroupId = request.UserGroupId};

            var members = await _objectManager.GetUserGroupMembers(request.UserGroupId);

            var res = new UserGroupUsersInfoResults
            {
                UserGroupId = info.SubjectId,
            };
            res.MemberUserId.AddRange(members);

            return res;
        }

        public override async Task<UserGroupInfoResults> EnsureUserGroup(EnsureUserGroupParams request,
            ServerCallContext context)
        {
            await _objectManager.EnsureUserGroup(
                request.UserGroupId,
                request.Description,
                request.ExternalIntId == 0?null:request.ExternalIntId, 
                string.IsNullOrEmpty(request.ExternalStrId)?null:request.ExternalStrId);

            var res = new UserGroupInfoResults
            {
                UserGroupId = request.UserGroupId,
                Description = request.Description
            };
            return res;
        }

        public override async Task<NoResults> DeleteUserGroup(DeleteUserGroupParams request, ServerCallContext context)
        {
            await _objectManager.DeleteUserGroup(request.UserGroupId, request.ForceDelete);
            return new NoResults();
        }

        public override async Task<NoResults> AddUserToGroupById(AddUserToGroupParams request,
            ServerCallContext context)
        {
            foreach (var uid in request.UserId)
            {
                await _objectManager.AddUserToUserGroup(request.UserGroupId, uid);

            }
            return new NoResults();
        }

        public override async Task<NoResults> RemoveUserFromGroupById(RemoveUserFromGroupParams request,
            ServerCallContext context)
        {
            foreach (var uid in request.UserId)
            {
                await _objectManager.RemoveUserFromUserGroup(request.UserGroupId, uid);
            }

            return new NoResults();
        }

        public override async Task<EffectivePermissionsResults> GetEffectivePermissions(GetEffectivePermissionsParams request,
            ServerCallContext context)
        {
            if (request.UserId == 0)
                throw new ObacException("UserId must be set");

            if (request.ObjectId == 0)
                throw new ObacException("ObjectId must be set");
           
            var ot = request.ObjectType.ToGuid();
            
            var res = new EffectivePermissionsResults
            {
                UserId = request.UserId,
                ObjectType = request.ObjectType,
                ObjectId = request.ObjectId
            };

            var permChecker = GetPermissionChecker(request.UserId);
            var ep = await permChecker.GetObjectPermissions(request.ObjectType.ToGuid(), request.ObjectId);
            res.EffectivePermissions.AddRange(ep.Select(p => p.ToGrpcUuid()));
            
            return res;
        }

        public override async Task<ResolveExternalIdsResults> ResolveExternalIds(ResolveExternalIdsParams request, ServerCallContext context)
        {
            var requestGrouped = request.Items.GroupBy(i => i.Type);
            var res = new ResolveExternalIdsResults();

            foreach (var itemGroup in requestGrouped)
            {
                var groupResolved = itemGroup.Key switch
                {
                    ResolveExternalItemType.ItemUser => await ResolveExternalUsers(itemGroup.ToArray()),
                    ResolveExternalItemType.ItemUserGroup => await ResolveExternalUserGroups(itemGroup.ToArray()),
                    ResolveExternalItemType.ItemTree => await ResolveExternalTrees(itemGroup.ToArray()),
                    ResolveExternalItemType.ItemTreeNode => await ResolveExternalTreeNodes(itemGroup.ToArray()),
                    _ => new List<ResolveExternalIdsResults.Types.ResolveExternalItemResult>()
                };
                
                if (groupResolved.Any())
                    res.Items.AddRange(groupResolved);
            }
            
            return res;
        }

        #endregion

        private async Task<IReadOnlyCollection<ResolveExternalIdsResults.Types.ResolveExternalItemResult>> ResolveExternalUsers(ResolveExternalItem[] items)
        {
            var res = new List<ResolveExternalIdsResults.Types.ResolveExternalItemResult>();
            foreach (var userItem in items)
            {
                var u = await _objectManager.GetUser(
                    null, 
                    userItem.ExternalIntId == 0? null: userItem.ExternalIntId, 
                    string.IsNullOrEmpty(userItem.ExternalStringId)?null:userItem.ExternalStringId);
                
                if (u == null)
                {
                    res.Add(new ResolveExternalIdsResults.Types.ResolveExternalItemResult
                    {
                        ExternalItem = userItem, Success = false
                    });
                }
                else
                {
                    res.Add(new ResolveExternalIdsResults.Types.ResolveExternalItemResult
                    {
                        ExternalItem = userItem, Success = true, ItemId = u.SubjectId
                    });
                }
            }
            
            return res;
        }
        
        private async Task<IReadOnlyCollection<ResolveExternalIdsResults.Types.ResolveExternalItemResult>> ResolveExternalUserGroups(ResolveExternalItem[] items)
        {
            var res = new List<ResolveExternalIdsResults.Types.ResolveExternalItemResult>();
            foreach (var userItem in items)
            {
                var u = await _objectManager.GetUserGroup(
                    null, 
                    userItem.ExternalIntId == 0? null: userItem.ExternalIntId, 
                    string.IsNullOrEmpty(userItem.ExternalStringId)?null:userItem.ExternalStringId);
                
                if (u == null)
                {
                    res.Add(new ResolveExternalIdsResults.Types.ResolveExternalItemResult
                    {
                        ExternalItem = userItem, Success = false
                    });
                }
                else
                {
                    res.Add(new ResolveExternalIdsResults.Types.ResolveExternalItemResult
                    {
                        ExternalItem = userItem, Success = true, ItemId = u.SubjectId
                    });
                }
            }
            
            return res;
        }
        
        private async Task<IReadOnlyCollection<ResolveExternalIdsResults.Types.ResolveExternalItemResult>> ResolveExternalTrees(ResolveExternalItem[] items)
        {
            var res = new List<ResolveExternalIdsResults.Types.ResolveExternalItemResult>();
            foreach (var userItem in items)
            {
                var u = await _objectManager.GetTree(
                    null, 
                    userItem.ExternalIntId == 0? null: userItem.ExternalIntId, 
                    string.IsNullOrEmpty(userItem.ExternalStringId)?null:userItem.ExternalStringId);
                
                if (u == null)
                {
                    res.Add(new ResolveExternalIdsResults.Types.ResolveExternalItemResult
                    {
                        ExternalItem = userItem, Success = false
                    });
                }
                else
                {
                    res.Add(new ResolveExternalIdsResults.Types.ResolveExternalItemResult
                    {
                        ExternalItem = userItem, Success = true, ItemIdGuid = u.TreeObjectTypeId.ToGrpcUuid()
                    });
                }
            }
            
            return res;
        }
        
        private async Task<IReadOnlyCollection<ResolveExternalIdsResults.Types.ResolveExternalItemResult>> ResolveExternalTreeNodes(ResolveExternalItem[] items)
        {
            var res = new List<ResolveExternalIdsResults.Types.ResolveExternalItemResult>();
            foreach (var userItem in items)
            {
                var u = await _objectManager.GetTreeNode(
                    userItem.NodeTypeId.ToGuid(),
                    null, 
                    userItem.ExternalIntId == 0? null: userItem.ExternalIntId, 
                    string.IsNullOrEmpty(userItem.ExternalStringId)?null:userItem.ExternalStringId);
                
                if (u == null)
                {
                    res.Add(new ResolveExternalIdsResults.Types.ResolveExternalItemResult
                    {
                        ExternalItem = userItem, Success = false
                    });
                }
                else
                {
                    res.Add(new ResolveExternalIdsResults.Types.ResolveExternalItemResult
                    {
                        ExternalItem = userItem, Success = true, ItemId = u.NodeId
                    });
                }
            }
            
            return res;
        }
        
        private IObacPermissionChecker GetPermissionChecker(int userId)
        {
            if (_permissionCheckers.TryGetValue(userId, out var pc))
                return pc;

            pc = _configuration.GetPermissionChecker(userId);
            _permissionCheckers[userId] = pc;
            return pc;
        }

        public override async Task<GetAclResults> GetAcl(GetAclParams request, ServerCallContext context)
        {
            var acl = await _objectManager.GetTreeNodeAcl(request.ObjectType.ToGuid(), request.ObjectId);

            return await AclToGrpcResponse(request, acl);
        }
        
        public override async Task<NoResults> SetAcl(SetAclParams request,
            ServerCallContext context)
        {
            CheckAcl(request);

            var acl = await GrpcRequestToAcl(request);
            
            await _objectManager.SetTreeNodeAcl(request.ObjectType.ToGuid(), request.ObjectId, acl);
            
            return new NoResults();
        }

        private async Task<AclInfo> GrpcRequestToAcl(SetAclParams request)
        {
            var res = new AclInfo
            {
                InheritParentPermissions = !request.DoNotInheritParentPermissions
            };
            var items = new List<AclItemInfo>();
            
            foreach (var aclItem in request.Acl)
            {
                AclItemInfo itemToAdd;
                if (aclItem.SubjectType == AclItemParams.Types.SubjectTypeEnum.User)
                {
                    var userObj = await _objectManager.GetUser(aclItem.UserId,
                        aclItem.ExternalUserIntId == 0?null:aclItem.ExternalUserIntId,
                        String.IsNullOrEmpty(aclItem.ExternalUserStringId)?null:aclItem.ExternalUserStringId);

                    if (userObj == null)
                        throw new ObacException(
                            $"subject type User cannot be found for id triade ({aclItem.UserId}, {aclItem.ExternalUserIntId}, {aclItem.ExternalUserStringId}");
                    
                    itemToAdd = new AclItemInfo
                    { 
                        UserId = userObj.SubjectId ,
                        PermissionType = aclItem.PermissionType== AclItemParams.Types.PermissionTypeEnum.Permission? AclItemInfoPermissionType.Permission: AclItemInfoPermissionType.Role,
                        PermissionId = aclItem.Permission.ToGuid(),
                        Kind = aclItem.DenyPermission ? PermissionKindEnum.Deny : PermissionKindEnum.Allow
                    };
                }
                else
                {
                    var userGroupObj = await _objectManager.GetUserGroup(aclItem.UserId,
                        aclItem.ExternalUserGroupIntId == 0? null: aclItem.ExternalUserGroupIntId,
                        String.IsNullOrEmpty(aclItem.ExternalUserGroupStringId)?null:aclItem.ExternalUserGroupStringId);

                    if (userGroupObj == null)
                        throw new ObacException(
                            $"subject type UserGroup cannot be found for id triade ({aclItem.UserGroupId}, {aclItem.ExternalUserGroupIntId}, {aclItem.ExternalUserGroupStringId}");

                    itemToAdd = new AclItemInfo
                    {
                        UserGroupId = userGroupObj.SubjectId ,
                        PermissionType = aclItem.PermissionType== AclItemParams.Types.PermissionTypeEnum.Permission? AclItemInfoPermissionType.Permission: AclItemInfoPermissionType.Role,
                        PermissionId = aclItem.Permission.ToGuid(),
                        Kind = aclItem.DenyPermission ? PermissionKindEnum.Deny : PermissionKindEnum.Allow
                    };
                }

                items.Add(itemToAdd);
            }

            res.AclItems = items.ToArray();
            
            return res;
        }
        
        private async Task<GetAclResults> AclToGrpcResponse(GetAclParams request, AclInfo acl)
        {
            var res = new GetAclResults
            {
                ObjectType = request.ObjectType, 
                ObjectId = request.ObjectId,
                DoNotInheritParentPermissions = !acl.InheritParentPermissions
            };

            foreach (var aclItem in acl.AclItems)
            {
                var aclUser =  aclItem.UserId.HasValue ? await _objectManager.GetUser(aclItem.UserId) : null;
                var aclUserGroup = aclItem.UserGroupId.HasValue ? await _objectManager.GetUserGroup(aclItem.UserGroupId) : null;

                var aclItemGrpc = new AclItemParams
                {
                    DenyPermission = aclItem.Kind == PermissionKindEnum.Deny,
                    Permission = aclItem.PermissionId.ToGrpcUuid(),
                    PermissionType = aclItem.PermissionType== AclItemInfoPermissionType.Permission? AclItemParams.Types.PermissionTypeEnum.Permission: AclItemParams.Types.PermissionTypeEnum.Role,
                    UserId = aclItem.UserId ?? 0,
                    ExternalUserIntId = aclUser?.ExternalIntId ?? 0,

                    UserGroupId = aclItem.UserGroupId ?? 0,
                    ExternalUserGroupIntId = aclUserGroup?.ExternalIntId ?? 0,
                };

                if (aclUser?.ExternalStringId != null)
                    aclItemGrpc.ExternalUserStringId = aclUser?.ExternalStringId;

                if (aclUserGroup?.ExternalStringId != null)
                    aclItemGrpc.ExternalUserGroupStringId = aclUserGroup?.ExternalStringId;

                        
                res.Acl.Add(aclItemGrpc);

            }

            return res;
        }

        private void CheckAcl(SetAclParams request)
        {
            if (request.ObjectId == 0)
                throw new ObacException("ObjectId must be set");

            // since we'd introduced External Int/Sting ids over here, this style of checking makes no sense
            // foreach (var aclItem in request.Acl)
            // {
            //     if (aclItem.UserId != 0 && aclItem.UserGroupId != 0)
            //         throw new ObacException("Cannot set both UserId or UserGroupId");
            //
            //     if (aclItem.UserId == 0 && aclItem.UserGroupId == 0)
            //         throw new ObacException("Either UserId or UserGroupId must be set");
            // }
        }
        
    }
}
        