using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface IRolePermissionService
    {
        List<Tbl_RolePermission> GetRolePermissions();
        List<Tbl_RolePermission> GetPermissionsByRoleTC(Guid roleTC);
        List<Tbl_RolePermission> GetRolesByPermissionTC(Guid permissionTC);
        bool AddRolePermission(Tbl_RolePermission rolePermission);
        bool EditRolePermission(Tbl_RolePermission rolePermission);
        bool DeleteRolePermission(Guid tc);
    }
}
