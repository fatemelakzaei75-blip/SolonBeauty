using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface IPersonalService
    {
        List<Tbl_Personal> GetPersonals();
        Tbl_Personal GetPersonalByToken(string token);
        bool IsPersonalExists(string token);
        List<Tbl_Personal> SearchPersonals(string search);
        bool AddPersonal(Tbl_Personal personal);
        bool EditPersonal(Tbl_Personal personal);
        bool DeletePersonal(string token);
    }
}
