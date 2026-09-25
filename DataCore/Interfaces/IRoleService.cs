using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface IRoleService
    {
        List<Tbl_Roles> GetRoles();
        Tbl_Roles GetRoleById(string Tc);
        List<Tbl_Roles> SearchRoles(string search);
        bool AddRole(Tbl_Roles role);
        bool EditRole(Tbl_Roles role);
        bool DeleteRole(string Tc);
    }
}
