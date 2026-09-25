using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت نقش‌های کاربری
/// </summary>
public class RoleService : BaseRepository<Tbl_Roles>, IRoleService
{
    public RoleService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_Roles> GetRoles()
    {
        return GetAll();
    }

    public Tbl_Roles GetRoleById(string Tc)
    {
        return GetByTc(Tc)!;
    }

    public List<Tbl_Roles> SearchRoles(string search)
    {
        return QueryNoTracking
            .Where(x => x.RoleName.Contains(search) || (x.Description ?? "").Contains(search))
            .ToList();
    }

    public bool AddRole(Tbl_Roles role)
    {
        return Insert(role);
    }

    public bool EditRole(Tbl_Roles role)
    {
        return Modify(role);
    }

    public bool DeleteRole(string Tc)
    {
        return SoftDelete(Tc);
    }
}
