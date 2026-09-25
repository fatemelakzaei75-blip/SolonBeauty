using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface ISalonService
    {
        Tbl_Salon GetSalon();
        bool AddSalon(Tbl_Salon salon);
        bool EditSalon(Tbl_Salon salon);
    }
}
