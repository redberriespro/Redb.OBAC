using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Ep;

namespace Redb.OBAC.Client.EffectivePermissionsReceiver
{
    public class EffectivePermissionsEfReceiver : IEffectivePermissionFeed
    {
        private readonly Func<IObacEpContext> _obacEpContext;
        private const int EP_BATCH_SZ = 100;

        public EffectivePermissionsEfReceiver(Func<IObacEpContext> obacEpContext)
        {
            _obacEpContext = obacEpContext;
        }

        public async Task FeedWithActionList(IEnumerable<PermissionActionInfo> actions)
        {
            // todo improve
            int n = EP_BATCH_SZ;

            var cxt = _obacEpContext();

            foreach (var a in actions)
            {
                switch (a.Action)
                {
                    case PermissionActionEnum.RemoveAllObjectsDirectPermission:
                    {
                        await cxt.DropEffectivePermissions(a.ObjectTypeId, a.ObjectId);
                    }
                        break;

                    case PermissionActionEnum.RemoveDirectPermission:
                    {
                        var p = await cxt
                            .EffectivePermissions
                            .SingleOrDefaultAsync(
                                p => p.ObjectTypeId == a.ObjectTypeId
                                     && p.ObjectId == a.ObjectId
                                     && p.PermissionId == a.PermissionId
                                     && p.UserId == a.UserId);
                        if (p != null)
                        {
                            cxt.EffectivePermissions.Remove(p);
                        }
                    }
                        break;
                    case PermissionActionEnum.AddDirectPermission:
                    {
                        await cxt.EffectivePermissions.AddAsync(new ObacEffectivePermissionsEntity()
                        {
                            Id = new Guid(),
                            PermissionId = a.PermissionId,
                            ObjectTypeId = a.ObjectTypeId,
                            ObjectId = a.ObjectId,
                            UserId = a.UserId
                        });
                    }
                        break;

                    default: throw new ApplicationException($"missing action branch: {a.Action}");
                }

                n -= 1;
                if (n > 0) continue;

                n = EP_BATCH_SZ;
                await cxt.SaveChangesAsync();
            }

            if (n != EP_BATCH_SZ)
                await cxt.SaveChangesAsync();
        }
    }
}