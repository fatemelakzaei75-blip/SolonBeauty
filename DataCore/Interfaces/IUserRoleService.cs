using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface IUserRoleService
    {
        bool AddUserRole(Tbl_UserRole userRole);
        bool EditUserRole(Tbl_UserRole userRole);
        bool DeleteUserRole(string tc);
        List<Tbl_UserRole> SearchByRoleTC(string roleTc);
    }
}
