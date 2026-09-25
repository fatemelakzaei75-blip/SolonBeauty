using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس دسترسی‌های سیستم
/// </summary>
public class PermissionService : BaseRepository<Tbl_Permission>, IPermissionService
{
    public PermissionService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_Permission> GetPermissions()
    {
        return GetAll();
    }

    public Tbl_Permission GetPermissionByTC(string tc)
    {
        return GetByTc(tc)!;
    }

    public List<Tbl_Permission> SearchPermissions(string search)
    {
        return QueryNoTracking
            .Where(x => x.PermissionName.Contains(search))
            .ToList();
    }

    public bool AddPermission(Tbl_Permission permission)
    {
        return Insert(permission);
    }

    public bool EditPermission(Tbl_Permission permission)
    {
        return Modify(permission);
    }

    public bool DeletePermission(string tc)
    {
        return SoftDelete(tc);
    }
}
