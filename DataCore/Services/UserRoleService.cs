using System;
using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس تخصیص نقش‌ها به کاربران
/// </summary>
public class UserRoleService : BaseRepository<Tbl_UserRole>, IUserRoleService
{
    public UserRoleService(DatabaseContext db) : base(db)
    {
    }

    public bool AddUserRole(Tbl_UserRole userRole)
    {
        return Insert(userRole);
    }

    public bool EditUserRole(Tbl_UserRole userRole)
    {
        return Modify(userRole);
    }

    public bool DeleteUserRole(string tc)
    {
        return SoftDelete(tc);
    }

    public List<Tbl_UserRole> SearchByRoleTC(string roleTc)
    {
        return Guid.TryParse(roleTc, out var g)
            ? QueryNoTracking.Where(x => x.RoleTC == g).ToList()
            : new List<Tbl_UserRole>();
    }

    public List<string> GetRoleNamesByPersonalTc(Guid personalTc)
    {
        return (from ur in QueryNoTracking
                join r in Db.Tbl_Roles on ur.RoleTC equals r.Tc
                where ur.PersonalTC == personalTc && !r.IsDelete
                select r.RoleName).ToList();
    }
}
