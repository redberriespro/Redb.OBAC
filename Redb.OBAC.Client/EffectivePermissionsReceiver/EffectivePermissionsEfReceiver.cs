using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Ep;

namespace Redb.OBAC.Client.EffectivePermissionsReceiver
{
    public class EffectivePermissionsEfReceiver: IEffectivePermissionFeed
    {
        private readonly ObacEpContextBase _epContext;
        private const int EP_BATCH_SZ = 100;
        
        public EffectivePermissionsEfReceiver(IEffectivePermissionsAware epContext)
        {
            _epContext = epContext as ObacEpContextBase;
        }
        public async Task FeedWithActionList(IEnumerable<PermissionActionInfo> actions)
        {
            var ctx = _epContext;
            // todo improve
            int n = EP_BATCH_SZ;
            
            foreach (var a in actions)
            {
                switch (a.Action)
                {
                    case PermissionActionEnum.RemoveAllObjectsDirectPermission:
                    {
                        await ctx.DropEffectivePermissions(a.ObjectTypeId, a.ObjectId);
                    } break;
                    
                    case PermissionActionEnum.RemoveDirectPermission:
                    {
                        var p = await ctx
                            .EffectivePermissions
                            .SingleOrDefaultAsync(
                                p=> p.ObjectTypeId == a.ObjectTypeId 
                                    && p.ObjectId == a.ObjectId
                                    && p.PermissionId == a.PermissionId
                                    && p.UserId == a.UserId);
                        if (p != null)
                        {
                            ctx.EffectivePermissions.Remove(p);
                        }
                    } break;
                    case PermissionActionEnum.AddDirectPermission:
                    {
                        await ctx.EffectivePermissions.AddAsync(new ObacEffectivePermissionsEntity()
                        {
                            Id = new Guid(),
                            PermissionId = a.PermissionId,
                            ObjectTypeId = a.ObjectTypeId,
                            ObjectId = a.ObjectId,
                            UserId = a.UserId
                        });
                    } break;
                    
                    default: throw new ApplicationException($"missing action branch: {a.Action}");
                }

                n -= 1;
                if (n > 0) continue;
                
                n = EP_BATCH_SZ;
                await ctx.SaveChangesAsync();
            }
            
            if (n != EP_BATCH_SZ)
                await ctx.SaveChangesAsync();        }
    }
}