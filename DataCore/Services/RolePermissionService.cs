using System;
using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس تخصیص دسترسی‌ها به نقش‌ها
/// </summary>
public class RolePermissionService : BaseRepository<Tbl_RolePermission>, IRolePermissionService
{
    public RolePermissionService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_RolePermission> GetRolePermissions()
    {
        return GetAll();
    }

    public List<Tbl_RolePermission> GetPermissionsByRoleTC(Guid roleTC)
    {
        return QueryNoTracking
            .Where(x => x.Tc == roleTC)
            .ToList();
    }

    public List<Tbl_RolePermission> GetRolesByPermissionTC(Guid permissionTC)
    {
        return QueryNoTracking
            .Where(x => x.PermissionTC == permissionTC)
            .ToList();
    }

    public bool AddRolePermission(Tbl_RolePermission rolePermission)
    {
        return Insert(rolePermission);
    }

    public bool EditRolePermission(Tbl_RolePermission rolePermission)
    {
        return Modify(rolePermission);
    }

    public bool DeleteRolePermission(Guid tc)
    {
        return SoftDelete(tc);
    }
}
