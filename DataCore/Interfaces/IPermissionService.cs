using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface IPermissionService
    {
        List<Tbl_Permission> GetPermissions();
        Tbl_Permission GetPermissionByTC(string tc);
        List<Tbl_Permission> SearchPermissions(string search);
        bool AddPermission(Tbl_Permission permission);
        bool EditPermission(Tbl_Permission permission);
        bool DeletePermission(string tc);
    }
}
